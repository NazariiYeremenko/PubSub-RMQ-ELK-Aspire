namespace Consumer.Models;

public class RabbitMqOptions
{
    public const string RbtMqOptions = "RbtMqOptions";
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string GuestCredit { get; set; } = string.Empty;
    public int ServiceLifetimeInMilliseconds { get; set; } = 0;
    public List<string> QueueNames { get; set; } = [];
    public string ExchangeName { get; set; } = string.Empty;
}
