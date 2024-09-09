namespace Consumer.Models;

public class ConnectionOptions
{
    public const string ConnectionStrings = "ConnectionStrings";
    
    public string RMQConnectionUri { get; set; } = string.Empty;
    
    public string RMQWebConnection { get; set; } = string.Empty;
    
    public string RMQHealthCheckUIConnection { get; set; } = string.Empty;
    
    public string LogstashConnection { get; set; } = string.Empty;
}