using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Consumer;

public class Consumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly HashSet<string> _receivedMessages;
    private readonly string _queueName;
    private readonly EventingBasicConsumer _consumer;
    private bool _disposed = false;

    public Consumer(RabbitMqOptions rabbitMqOptions, int consumerId)
    {
        ArgumentNullException.ThrowIfNull(rabbitMqOptions);
        ArgumentNullException.ThrowIfNull(consumerId);

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.GuestCredit,
            Password = rabbitMqOptions.GuestCredit,
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _receivedMessages = [];
        _queueName = rabbitMqOptions.QueueNames[consumerId];

        _channel.ExchangeDeclare(exchange: rabbitMqOptions.ExchangeName, type: ExchangeType.Fanout);

        _channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );
       _channel.QueueBind(queue: _queueName,
                exchange: rabbitMqOptions.ExchangeName,
                routingKey: "");

        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Duplicated messages check
                if (!_receivedMessages.Contains(message))
                {
                    Console.WriteLine($"Consumer {consumerId} received a message from {ea.RoutingKey}: {message}");
                    _receivedMessages.Add(message);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred in Consumer {consumerId} - received handler: {ex.Message}");

                // Reject the message and requeue it
                _channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
            }
        };

        _channel.BasicConsume(
                queue: _queueName,
                autoAck: false, // Using manual acknowledgments
                consumer: _consumer
                );
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

    ~Consumer()
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
                _channel?.Dispose();
                _connection?.Dispose();
            }

            _disposed = true;
        }
    }
}