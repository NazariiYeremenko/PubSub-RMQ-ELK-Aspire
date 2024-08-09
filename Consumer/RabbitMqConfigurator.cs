using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Consumer;

public static class RabbitMqConfigurator
{
    public static RabbitMqOptions ConfigureRabbitMqOptions(HostApplicationBuilder builder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Construct the absolute path to the appsettings.json file
        var appSettingsPath = Path.Combine(baseDirectory, "appsettings.json");

        builder.Configuration
            .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);

        // Bind the RabbitMqOptions section from appsettings.json to the RabbitMqOptions class model
        var rabbitMqOptions = new RabbitMqOptions();
        builder.Configuration.GetSection(RabbitMqOptions.RbtMqOptions).Bind(rabbitMqOptions);

        return rabbitMqOptions;
    }
}
