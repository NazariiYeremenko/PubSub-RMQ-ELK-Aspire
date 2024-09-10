using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(sp =>
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqp://guest:guest@rabbitmqservice:5672/"),
            AutomaticRecoveryEnabled = true
        };

        return factory.CreateConnection();
    })
    .AddHealthChecks()
    .AddRabbitMQ();


var apiservice = builder.AddProject<Projects.NetAspireApp_ApiService>("apiservice")
    .WithReference("external-api", new Uri("https://webhook.site"));

builder.AddProject<Projects.NetAspireApp_Web>("webfrontend")
    .WithReference(apiservice);


builder.Build().Run();
    