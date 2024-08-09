using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Consumer;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;

namespace Producer;

internal class Program
{
    private const string UriString = "tcp://127.0.0.1:23750";
    private const string LogstashContainerPath = "http://docker-elk-logstash-1:50000";
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();

        builder.Services
        .AddSingleton(sp =>
         {
             var factory = new ConnectionFactory
             {
                 Uri = new Uri(UriString),
                 AutomaticRecoveryEnabled = true
             };
             return factory.CreateConnection();
         })
        .AddHealthChecks()
        .AddRabbitMQ();

        var rabbitMqOptions = RabbitMqConfigurator.ConfigureRabbitMqOptions(builder);

        var queuesAmount = rabbitMqOptions.QueueNames.Count;

        var tasks = new Task[queuesAmount];

        for (int i = 0; i < queuesAmount; i++)
        {
            var producer = new Producer(rabbitMqOptions, i);
            tasks[i] = Task.Run(() => RunProducer(producer, rabbitMqOptions.QueueNames[i-1], rabbitMqOptions.ServiceLifetimeInMiliseconds));
        }
        await Task.WhenAll(tasks);
    }

    static async Task RunProducer(Producer producer, string queueName, int serviceLifetime)
    {
        using var timer = new Timer((_) =>
        {
            var healthCheck = producer.CheckHealth();
            Console.WriteLine(healthCheck.Description);
            if(healthCheck.Status is HealthStatus.Unhealthy)
            {
                producer.Dispose();
/*                Environment.Exit(1);
*/          }
            var messageTemplate = $"This is sample message to {queueName}";
            producer.PublishMessage(messageTemplate, queueName);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Producer", "Serilog.Sinks.LogstashHttp")
                .WriteTo.LogstashHttp(LogstashContainerPath)
                .CreateLogger();
            Log.Information(messageTemplate, queueName);
            Log.CloseAndFlush();

        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        // Wait for a specified duration
        await Task.Delay(TimeSpan.FromMilliseconds(serviceLifetime));

        producer.Dispose();
    }
}