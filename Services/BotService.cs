using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VKBot.Web.Models;

namespace VKBot.Web.Services
{
    public class BotService : BackgroundService
    {
        private readonly ILogger<BotService> _log;
        private readonly IHttpClientFactory _http;
        private readonly VkSettings _vk;
        private readonly ErrorLogger _errors;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ConcurrentDictionary<long, (string date, string session)> _userSelectedData = new();

        public BotService(ILogger<BotService> log, IHttpClientFactory http, IOptions<VkSettings> vkOptions, ErrorLogger errors)
        {
            _log = log;
            _http = http;
            _vk = vkOptions.Value;
            _errors = errors;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(_vk.AccessToken))
            {
                _log.LogError("Vk:AccessToken is not configured. Set it in appsettings.json or environment."); 
                return;
            }

            if (string.IsNullOrEmpty(_vk.GroupId))
            {
                _log.LogWarning("Vk:GroupId not configured. LongPoll may fail."); 
            }

            var client = _http.CreateClient("vkclient");

            try
            {
                _log.LogInformation("Getting LongPoll server...");

                var serverResp = await client.GetFromJsonAsync<LongPollServerResponse>(
                    $"https://api.vk.com/method/groups.getLongPollServer?group_id={_vk.GroupId}&access_token={_vk.AccessToken}&v={_vk.ApiVersion}",
                    _jsonOptions, stoppingToken);

                if (serverResp?.Response == null)
                {
                    _log.LogError("Failed to get LongPoll server response.");
                    return;
                }

                string server = serverResp.Response.Server;
                string key = serverResp.Response.Key;
                string ts = serverResp.Response.Ts;

                _log.LogInformation("LongPoll initialized. Listening for events...");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var pollStr = await client.GetStringAsync($"{server}?act=a_check&key={key}&ts={ts}&wait=25", stoppingToken);
                        var poll = JsonSerializer.Deserialize<LongPollUpdate>(pollStr, _jsonOptions);
                        if (poll == null) continue;
                        ts = poll.Ts ?? ts;
                        if (poll.Updates?.Length > 0)
                        {
                            foreach (var u in poll.Updates)
                            {
                                await ProcessUpdateAsync(u, client);
                            }
                        }
                    }
                    catch (TaskCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "LongPoll loop error");
                        await _errors.LogErrorAsync(ex, "CRITICAL", additional: new { Component = "MainLoop" });
                        await Task.Delay(3000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Bot initialization failed");
                await _errors.LogErrorAsync(ex, "FATAL", additional: new { Component = "Initialization" });
            }
        }

        private async Task ProcessUpdateAsync(UpdateItem update, HttpClient client)
        {
            try
            {
                if (update.Type == "message_allow" && update.Object?.UserId != null)
                {
                    var uid = update.Object.UserId.Value;
                    var welcome = GenerateWelcomeText();
                    var keyboard = GenerateWelcomeKeyboard();
                    var url = BuildSendUrl(userId: uid, message: welcome, keyboardJson: keyboard);
                    await client.GetStringAsync(url);
                    return;
                }

                if (update.Type == "message_new" && update.Object?.Message != null)
                {
                    await ProcessMessageAsync(update.Object.Message, client);
                }
            }
            catch (Exception ex)
            {
                long? uid = update.Object?.UserId ?? update.Object?.Message?.FromId;
                await _errors.LogErrorAsync(ex, "ERROR", uid, additional: new { Update = update });
            }
        }

        private async Task ProcessMessageAsync(MessageItem message, HttpClient client)
        {
            var msg = message.Text ?? string.Empty;
            var userId = message.FromId;

            _log.LogInformation("Message from {user}: {text}", userId, msg);

            string reply = string.Empty;
            string? keyboard = null;

            try
            {
                if (IsTicketCategoryMessage(msg))
                {
                    if (_userSelectedData.TryGetValue(userId, out var td))
                    {
                        var category = GetTicketCategoryFromMessage(msg);
                        var (m, k) = await GetFormattedTariffsAsync(client, td.date, td.session, category);
                        reply = m; keyboard = k;
                    }
                    else
                    {
                        reply = "Сначала выберите дату и сеанс 📅"; keyboard = TicketsDateKeyboard();
                    }
                }
                else
                {
                    switch (msg.ToLowerInvariant())
                    {
                        case "/start": case "начать": case "🚀 начать":
                            reply = "Добро пожаловать! Выберите пункт 👇"; keyboard = MainMenuKeyboard(); break;
                        case "информация": case "ℹ️ информация":
                            reply = "Выберите интересующую информацию 👇"; keyboard = InfoMenuKeyboard(); break;
                        case "время работы": case "⏰ время работы": reply = GetWorkingHours(); break;
                        case "контакты": case "📞 контакты": reply = GetContacts(); break;
                        case "🔙 назад": case "назад": reply = "Главное меню:"; keyboard = MainMenuKeyboard(); _userSelectedData.TryRemove(userId, out _); break;
                        case "🔙 к сеансам":
                            if (_userSelectedData.TryGetValue(userId, out var sd))
                            {
                                var (m,k) = await GetSessionsForDateAsync(client, sd.date);
                                reply = m; keyboard = k;
                            }
                            else { reply = "Выберите дату для сеанса:"; keyboard = TicketsDateKeyboard(); }
                            break;
                        case "🔙 в начало": reply = "Главное меню:"; keyboard = MainMenuKeyboard(); _userSelectedData.TryRemove(userId, out _); break;
                        case "🎟 купить билеты": case "билеты": reply = "Выберите дату для сеанса:"; keyboard = TicketsDateKeyboard(); break;
                        case "📊 загруженность": case "загруженность": reply = await GetParkLoadAsync(client); break;
                        default:
                            if (msg.StartsWith("📅"))
                            {
                                var date = msg.Replace("📅", "").Trim();
                                var (m,k) = await GetSessionsForDateAsync(client, date);
                                reply = m; keyboard = k;
                                _userSelectedData[userId] = (date, "");
                            }
                            else if (msg.StartsWith("⏰"))
                            {
                                var session = msg.Replace("⏰", "").Trim();
                                if (!_userSelectedData.TryGetValue(userId, out var cur))
                                {
                                    reply = "Сначала выберите дату 📅"; keyboard = TicketsDateKeyboard();
                                }
                                else
                                {
                                    _userSelectedData[userId] = (cur.date, session);
                                    reply = $"🎟 *Сеанс: {session} ({cur.date})*\n\nВыберите категорию билетов:";
                                    keyboard = TicketCategoryKeyboard();
                                }
                            }
                            else { reply = "Я вас не понял, попробуйте еще раз 😅"; }
                            break;
                    }
                }

                var url = BuildSendUrl(userId: userId, message: reply, keyboardJson: keyboard);
                await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                await _errors.LogErrorAsync(ex, "ERROR", userId, additional: new { Message = msg, HasSelected = _userSelectedData.ContainsKey(userId) });
                var errMsg = "Произошла ошибка при обработке запроса. Мы уже работаем над этим! 🛠️";
                var errUrl = BuildSendUrl(userId: userId, message: errMsg);
                await client.GetStringAsync(errUrl);
            }
        }

        // Utilities: Build VK send URL (keeps token in config)
        private string BuildSendUrl(long userId, string message, string? keyboardJson = null)
        {
            var token = _vk.AccessToken;
            var v = _vk.ApiVersion ?? "5.131";
            var url = $"https://api.vk.com/method/messages.send?user_id={userId}&random_id={Environment.TickCount}&message={Uri.EscapeDataString(message)}&access_token={token}&v={v}";
            if (!string.IsNullOrEmpty(keyboardJson)) url += $"&keyboard={Uri.EscapeDataString(keyboardJson)}";
            return url;
        }

        // The rest of helper methods are ported (IsTicketCategoryMessage, keyboards, GetParkLoadAsync, GetSessionsForDateAsync, GetFormattedTariffsAsync, etc.)
        // For brevity these helper methods are implemented below — they are adapted from the original Program.cs logic.

        // --- helper methods (copied/adapted) ---

        private static bool IsTicketCategoryMessage(string message)
        {
            var lowerMsg = message.ToLowerInvariant();
            return lowerMsg.Contains("взрос") ||
                   lowerMsg.Contains("детск") ||
                   lowerMsg.Contains("adult") ||
                   lowerMsg.Contains("child") ||
                   lowerMsg.Contains("kids") ||
                   lowerMsg == "👤" || lowerMsg == "👶" ||
                   lowerMsg == "взрослые" || lowerMsg == "детские";
        }

        private static string GetTicketCategoryFromMessage(string message)
        {
            var lowerMsg = message.ToLowerInvariant();
            return (lowerMsg.Contains("взрос") || lowerMsg.Contains("adult") || lowerMsg == "👤") ? "adult" : "child";
        }

        private static string MainMenuKeyboard() => JsonSerializer.Serialize(new
        {
            one_time = false,
            buttons = new[] {
                new[] {
                    new { action = new { type = "text", label = "ℹ️ Информация" }, color = "primary" },
                    new { action = new { type = "text", label = "🎟 Купить билеты" }, color = "positive" },
                    new { action = new { type = "text", label = "📊 Загруженность" }, color = "secondary" }
                }
            }
        });

        private static string InfoMenuKeyboard() => JsonSerializer.Serialize(new
        {
            one_time = false,
            buttons = new[] {
                new[] {
                    new { action = new { type = "text", label = "⏰ Время работы" }, color = "primary" },
                    new { action = new { type = "text", label = "📞 Контакты" }, color = "primary" }
                },
                new[] {
                    new { action = new { type = "text", label = "🔙 Назад" }, color = "negative" }
                }
            }
        });

        private static string TicketsDateKeyboard()
        {
            var buttons = new List<object[]>();
            var row1 = new List<object>();
            for (int i = 0; i < 3; i++)
            {
                string dateStr = DateTime.Now.AddDays(i).ToString("dd.MM.yyyy");
                row1.Add(new { action = new { type = "text", label = $"📅 {dateStr}" }, color = "primary" });
            }
            buttons.Add(row1.ToArray());

            var row2 = new List<object>();
            for (int i = 3; i < 5; i++)
            {
                string dateStr = DateTime.Now.AddDays(i).ToString("dd.MM.yyyy");
                row2.Add(new { action = new { type = "text", label = $"📅 {dateStr}" }, color = "primary" });
            }
            buttons.Add(row2.ToArray());

            buttons.Add(new object[] { new { action = new { type = "text", label = "🔙 Назад" }, color = "negative" } });
            return JsonSerializer.Serialize(new { one_time = true, buttons = buttons });
        }

        private static string TicketCategoryKeyboard() => JsonSerializer.Serialize(new
        {
            one_time = true,
            buttons = new[]
            {
                new[] {
                    new { action = new { type = "text", label = "👤 Взрослые билеты" }, color = "primary" },
                    new { action = new { type = "text", label = "👶 Детские билеты" }, color = "positive" }
                },
                new[] {
                    new { action = new { type = "text", label = "🔙 Назад" }, color = "negative" }
                }
            }
        });

        private static string BackKeyboard() => JsonSerializer.Serialize(new
        {
            one_time = true,
            buttons = new[] { new[] { new { action = new { type = "text", label = "🔙 Назад" }, color = "negative" } } }
        });

        private static string GenerateWelcomeKeyboard() => JsonSerializer.Serialize(new
        {
            one_time = true,
            buttons = new[] { new[] { new { action = new { type = "text", label = "🚀 Начать" }, color = "positive" } } }
        );

        private string GenerateWelcomeText() => string.Join("\n", new[] {
            "🌊 ДОБРО ПОЛОЖАЛОВАТЬ В ЦЕНТР YES!",
            "Я ваш персональный помощник для организации незабываемого отдыха! 🎯",
            "🎟 УМНАЯ ПОКУПКА БИЛЕТОВ - выбор даты, сеанса и тарифов.",
            "📊 ОНЛАЙН-МОНИТОРИНГ ЗАГРУЖЕННОСТИ - реальная картина посещаемости.",
            "ℹ️ ПОЛНАЯ ИНФОРМАЦИЯ О ЦЕНТРЕ - расписание, контакты и т.д.",
            "🚀 Начните прямо сейчас! Выберите раздел в меню ниже."
        });

        private async Task<string> GetParkLoadAsync(HttpClient client)
        {
            try
            {
                var requestData = new { SiteID = "1" };
                var response = await client.PostAsJsonAsync("https://apigateway.nordciti.ru/v1/aqua/CurrentLoad", requestData);
                if (!response.IsSuccessStatusCode) return "Не удалось получить данные о загруженности 😔";
                var data = await response.Content.ReadFromJsonAsync<ParkLoadResponse>(_jsonOptions);
                if (data == null) return "Не удалось обработать ответ 😔";
                string loadStatus = data.Load switch { < 30 => "Мало людей 🟢", < 70 => "Средняя загруженность 🟡", _ => "Много людей 🔴" };
                return $"📊 Загруженность аквапарка:\n\n👥 В данный {data.Count} человек\n📈 {data.Load}% ({loadStatus})";
            }
            catch (Exception ex) { await _errors.LogErrorAsync(ex, additional: new { Component = "GetParkLoad" }); return "Ошибка при получении загруженности 😔"; }
        }

        private async Task<(string message, string keyboard)> GetSessionsForDateAsync(HttpClient client, string date)
        {
            try
            {
                var sessionsUrl = $"https://apigateway.nordciti.ru/v1/aqua/getSessionsAqua?date={date}";
                var sessionsResponse = await client.GetAsync(sessionsUrl);
                if (!sessionsResponse.IsSuccessStatusCode) return ($"⚠️ Ошибка при загрузке сеансов на {date}", TicketsDateKeyboard());
                var sessionsJson = await sessionsResponse.Content.ReadAsStringAsync();
                var sessionsData = JsonSerializer.Deserialize<JsonElement>(sessionsJson);
                if (!sessionsData.TryGetProperty("result", out var sessionsArray) || sessionsArray.GetArrayLength() == 0) return ($"😔 На {date} нет доступных сеансов.", TicketsDateKeyboard());
                string text = $"🎟 *Доступные сеансы на {date}:*\n\n";
                var buttonsList = new List<object[]>();
                foreach (var s in sessionsArray.EnumerateArray())
                {
                    string timeStart = s.TryGetProperty("startTime", out var ts) ? ts.GetString() ?? "" : "";
                    string timeEnd = s.TryGetProperty("endTime", out var te) ? te.GetString() ?? "" : "";
                    int placesFree = s.TryGetProperty("availableCount", out var pf) ? pf.GetInt32() : 0;
                    int placesTotal = s.TryGetProperty("totalCount", out var pt) ? pt.GetInt32() : 0;
                    string sessionTime = s.TryGetProperty("sessionTime", out var st) ? st.GetString() ?? $"{timeStart}-{timeEnd}" : $"{timeStart}-{timeEnd}";
                    if (placesFree == 0) continue;
                    string availability = placesFree < 10 ? "🔴 Мало мест!" : "🟢 Есть места";
                    text += $"⏰ *{sessionTime}* | {availability}\n   Свободно: {placesFree}/{placesTotal} мест\n\n";
                    buttonsList.Add(new object[] { new { action = new { type = "text", label = $"⏰ {sessionTime}" }, color = "primary" } });
                }
                if (buttonsList.Count == 0) return ($"😔 На {date} нет свободных мест.", TicketsDateKeyboard());
                buttonsList.Add(new object[] { new { action = new { type = "text", label = "🔙 Назад" }, color = "negative" } });
                string keyboard = JsonSerializer.Serialize(new { one_time = true, buttons = buttonsList });
                return (text, keyboard);
            }
            catch (Exception ex) { await _errors.LogErrorAsync(ex, additional: new { Component = "GetSessionsForDate" }); return ($"Ошибка при получении сеансов 😔\n{ex.Message}", TicketsDateKeyboard()); }
        }

        private async Task<(string message, string keyboard)> GetFormattedTariffsAsync(HttpClient client, string date, string sessionTime, string category)
        {
            try
            {
                var tariffsUrl = $"https://apigateway.nordciti.ru/v1/aqua/getTariffsAqua?date={date}";
                var tariffsResponse = await client.GetAsync(tariffsUrl);
                if (!tariffsResponse.IsSuccessStatusCode) return ($"⚠️ Ошибка при загрузке тарифов", BackKeyboard());
                var tariffsJson = await tariffsResponse.Content.ReadAsStringAsync();
                var tariffsData = JsonSerializer.Deserialize<JsonElement>(tariffsJson);
                if (!tariffsData.TryGetProperty("result", out var tariffsArray) || tariffsArray.GetArrayLength() == 0) return ($"⚠️ Не удалось получить тарифы", BackKeyboard());
                string categoryTitle = category == "adult" ? "👤 ВЗРОСЛЫЕ БИЛЕТЫ" : "👶 ДЕТСКИЕ БИЛЕТЫ";
                string text = $"🎟 *{categoryTitle}*\n⏰ Сеанс: {sessionTime}\n📅 Дата: {date}\n\n";
                var filteredTariffs = new List<(string name, decimal price)>();
                var seenTariffs = new HashSet<string>();
                foreach (var t in tariffsArray.EnumerateArray())
                {
                    string name = t.TryGetProperty("Name", out var n) ? n.GetString() ?? "" : "";
                    decimal price = t.TryGetProperty("Price", out var p) ? p.GetDecimal() : 0;
                    if (string.IsNullOrEmpty(name)) name = t.TryGetProperty("name", out var n2) ? n2.GetString() ?? "" : "";
                    if (price == 0) price = t.TryGetProperty("price", out var p2) ? p2.GetDecimal() : 0;
                    string tariffKey = $"{name.ToLowerInvariant()}_{price}";
                    if (seenTariffs.Contains(tariffKey)) continue;
                    seenTariffs.Add(tariffKey);
                    string nameLower = name.ToLowerInvariant();
                    bool isAdult = nameLower.Contains("взрос") || nameLower.Contains("adult") || (nameLower.Contains("вип") && !nameLower.Contains("дет")) || (nameLower.Contains("взр") && !nameLower.Contains("дет")) || (price > 1000 && !nameLower.Contains("дет"));
                    bool isChild = nameLower.Contains("детск") || nameLower.Contains("child") || nameLower.Contains("kids") || nameLower.Contains("дет") || (price < 1000 && nameLower.Contains("билет") && !nameLower.Contains("взр"));
                    if ((category == "adult" && isAdult && !isChild) || (category == "child" && isChild && !isAdult)) filteredTariffs.Add((name, price));
                }
                if (filteredTariffs.Count == 0) { text += "😔 Нет доступных билетов этой категории\n💡 Попробуйте выбрать другую категорию"; }
                else
                {
                    var groupedTariffs = filteredTariffs.GroupBy(t => FormatTicketName(t.name)).Select(g => g.First()).OrderByDescending(t => t.price).ToList();
                    foreach (var (name, price) in groupedTariffs)
                    {
                        string emoji = price > 2000 ? "💎 VIP" : price > 1000 ? "⭐ Стандарт" : "🎫 Эконом";
                        string formattedName = FormatTicketName(name);
                        text += $"{emoji} *{formattedName}*: {price}₽\n";
                    }
                    text += "\n💡 Примечания:\n• Детский билет - для детей от 4 до 12 лет\n• Дети до 4 лет - бесплатно (с взрослым)\n• VIP билеты включают дополнительные услуги";
                }
                text += "\n🔗 *Купить онлайн:* yes35.ru";
                string keyboard = JsonSerializer.Serialize(new
                {
                    one_time = false,
                    buttons = new object[][]
                    {
                        new object[] { new { action = new { type = "open_link", link = "https://yes35.ru/aquapark/tickets", label = "🎟 Купить на сайте" } } },
                        new object[] { new { action = new { type = "text", label = "👤 Взрослые" }, color = category == "adult" ? "positive" : "primary" }, new { action = new { type = "text", label = "👶 Детские" }, color = category == "child" ? "positive" : "primary" } },
                        new object[] { new { action = new { type = "text", label = "🔙 К сеансам" }, color = "secondary" }, new { action = new { type = "text", label = "🔙 В начало" }, color = "negative" } }
                    }
                });
                return (text, keyboard);
            }
            catch (Exception ex) { await _errors.LogErrorAsync(ex, additional: new { Component = "GetTariffs" }); return ($"Ошибка при получении тарифов 😔\n{ex.Message}", BackKeyboard()); }
        }

        private static string FormatTicketName(string name)
        {
            var formatted = name.Replace("Билет", "").Replace("билет", "").Replace("Вип", "VIP").Replace("весь день", "Весь день").Replace("взрослый", "").Replace("детский", "").Replace("вечерний", "Вечерний").Replace("  ", " ").Trim();
            if (formatted.StartsWith("VIP") || formatted.StartsWith("Вип")) formatted = "VIP" + formatted.Substring(3).Trim();
            return string.IsNullOrEmpty(formatted) ? "Стандартный" : formatted;
        }

        private static string GetWorkingHours() { return "🏢 Режим работы...\n(детализированный текст опущен для краткости)"; }
        private static string GetContacts() { return "📞 Контакты Центра YES\n\n• Основной: (8172) 33-06-06\n• Ресторан: 8-800-200-67-71\nyes@yes35.ru"; }

        // --- models used inside service ---
        public class ParkLoadResponse { public int Count { get; set; } public int Load { get; set; } }
        public class SessionResponse { public SessionItem[] Data { get; set; } = Array.Empty<SessionItem>(); }
        public class SessionItem { public string TimeStart { get; set; } = ""; public string TimeEnd { get; set; } = ""; public int PlacesFree { get; set; } public int PlacesTotal { get; set; } }
        public class TariffResponse { public TariffItem[] Data { get; set; } = Array.Empty<TariffItem>(); }
        public class TariffItem { public string Name { get; set; } = ""; public decimal Price { get; set; } }
    }
}
