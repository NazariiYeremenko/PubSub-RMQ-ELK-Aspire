using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using RabbitMQ.Client;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/*        builder.Services.AddControllersWithViews();

        builder.WebHost.UseUrls("https://localhost:443");

        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            var kestrelSection = context.Configuration.GetSection("Kestrel");

            serverOptions.Configure(kestrelSection)
                .Endpoint("HTTPS", listenOptions =>
                {
                    // ...
                });
        });*/


/*        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672/"),
            AutomaticRecoveryEnabled = true,          
        };

        var connection = factory.CreateConnection();
        builder.Services.AddSingleton(connection);

        // Add health checks
        builder.Services.AddHealthChecks()
            .AddRabbitMQ(options =>
            {
                options.Connection = connection;
            });*/


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


/*builder.Services.AddHealthChecksUI(settings => settings.AddHealthCheckEndpoint("My health checks", "http://rabbitmq-healthui-1:5000/healthchecks-ui/"));
*/
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapHealthChecks(
    "/health"/*,
new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
}*/);

// Map Health Checks UI
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/healthchecks-ui";  // Endpoint for Health Checks UI
    options.ApiPath = "/healthchecks-api"; // API endpoint for Health Checks UI
});

/*app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");*/

app.Run();
