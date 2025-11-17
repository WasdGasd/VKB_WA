namespace VKB_WA.Models
{
    public class BotCommand
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string ActionType { get; set; } = "";
        public string ActionData { get; set; } = "";
    }
}
