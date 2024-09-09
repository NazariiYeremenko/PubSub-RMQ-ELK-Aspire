using RabbitMQ.Client;
using System.Text;
using Consumer.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Producer;
public class Producer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed = false;
    private readonly string _queueName;

    public Producer(RabbitMqOptions rabbitMqOptions, int queueNumber)
    {
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.GuestCredit,
            Password = rabbitMqOptions.GuestCredit,
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _queueName = rabbitMqOptions.QueueNames[queueNumber];

        _channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public void PublishMessage(string message, string queueName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Producer), "Cannot publish message. The object has been disposed.");
        }

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body
        );

        Console.WriteLine($"[x] Sent a message : {message}");
    }
    public HealthCheckResult CheckHealth()
    {
        try
        {
            if (_disposed)
            {
                return HealthCheckResult.Unhealthy("Producer object is disposed.");
            }

            // Check RabbitMQ connection health
            if (!_connection.IsOpen || _connection.Endpoint is null)
            {
                return HealthCheckResult.Unhealthy($"RabbitMQ connection to {_queueName} is closed.");
            }

            return HealthCheckResult.Healthy($"Healthy connection to {_queueName}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    ~Producer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _channel?.Dispose();
                _connection?.Dispose();
            }

            // Dispose unmanaged resources (if any)

            _disposed = true; //ensure that the object is not disposed multiple times
        }
    }
}