using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Consumer;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();

        var rabbitMqOptions = ConstantsConfigurator.ConfigureRabbitMqOptions(builder);
            
        var consumers = new List<Consumer>();

        for (var consumerId = 0; consumerId < 3; consumerId++) 
        {
            var consumer = new Consumer(rabbitMqOptions, consumerId);
            Console.WriteLine(consumer.CheckHealth().Description);
            consumers.Add(consumer);
        }

        Thread.Sleep(rabbitMqOptions.ServiceLifetimeInMilliseconds);

        foreach (var consumer in consumers)
        {
            consumer.Dispose();
        }
    }
}