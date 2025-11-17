using System.Text.Json;

namespace VKBot.Web.Models
{
	public class ErrorLog
	{
		public DateTime Timestamp { get; set; }
		public string ErrorMessage { get; set; } = string.Empty;
		public long? UserId { get; set; }
		public string? Command { get; set; }
		public string? AdditionalData { get; set; }
	}
}
