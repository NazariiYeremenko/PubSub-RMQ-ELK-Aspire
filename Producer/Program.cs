using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Consumer;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;

namespace Producer;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();
        
        var connectionStrings = ConstantsConfigurator.ConfigureConnectionsOptions(builder);

        builder.Services
        .AddSingleton(sp =>
         {
             var factory = new ConnectionFactory
             {
                 Uri = new Uri(connectionStrings.RMQConnectionUri),
                 AutomaticRecoveryEnabled = true
             };
             return factory.CreateConnection();
         })
        .AddHealthChecks()
        .AddRabbitMQ();

        var rabbitMqOptions = ConstantsConfigurator.ConfigureRabbitMqOptions(builder);

        var queuesAmount = rabbitMqOptions.QueueNames.Count;

        var tasks = new Task[queuesAmount];

        for (var i = 0; i < queuesAmount; i++)
        {
            var producer = new Producer(rabbitMqOptions, i);
            tasks[i] = Task.Run(() => RunProducer(producer, rabbitMqOptions.QueueNames[i-1], rabbitMqOptions.ServiceLifetimeInMilliseconds, connectionStrings.LogstashConnection));
        }
        await Task.WhenAll(tasks);
    }

    private static async Task RunProducer(Producer producer, string queueName, int serviceLifetime, string logstashContainerPath)
    {
        await using var timer = new Timer((_) =>
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
                .WriteTo.LogstashHttp(logstashContainerPath)
                .CreateLogger();
            Log.Information(messageTemplate, queueName);
            Log.CloseAndFlush();

        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        // Wait for a specified duration
        await Task.Delay(TimeSpan.FromMilliseconds(serviceLifetime));

        producer.Dispose();
    }
}