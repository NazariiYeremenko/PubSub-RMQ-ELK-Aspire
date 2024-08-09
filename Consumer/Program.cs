using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Consumer;

internal class Program
{
    static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();

        var rabbitMqOptions = RabbitMqConfigurator.ConfigureRabbitMqOptions(builder);

        builder.Configuration.GetSection(RabbitMqOptions.RbtMqOptions).Bind(rabbitMqOptions);

        var consumers = new List<Consumer>();

        for (int consumerId = 0; consumerId < 3; consumerId++) 
        {
            var consumer = new Consumer(rabbitMqOptions, consumerId);
            Console.WriteLine(consumer.CheckHealth().Description);
            consumers.Add(consumer);
        }

        Thread.Sleep(rabbitMqOptions.ServiceLifetimeInMiliseconds);

        foreach (var consumer in consumers)
        {
            consumer.Dispose();
        }
    }
}