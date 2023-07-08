using Microsoft.Extensions.Logging;

namespace HammadBroker.Model;

public class LogMessage
{
	public LogLevel Level { get; set; }
	public string Message { get; set; }
}