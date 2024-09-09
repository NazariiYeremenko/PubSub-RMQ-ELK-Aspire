using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Consumer.Models;

namespace Consumer;

public static class ConstantsConfigurator
{
    private const string AppSettingsFileName = "appsettings.json";
    public static RabbitMqOptions ConfigureRabbitMqOptions(HostApplicationBuilder builder)
    {
        ConfigureFilePath(builder);

        // Bind the RabbitMqOptions section from appsettings.json to the RabbitMqOptions class model
        var rabbitMqOptions = new RabbitMqOptions();
        builder.Configuration.GetSection(RabbitMqOptions.RbtMqOptions).Bind(rabbitMqOptions);

        return rabbitMqOptions;
    }
    public static ConnectionOptions ConfigureConnectionsOptions(HostApplicationBuilder builder)
    {
        ConfigureFilePath(builder);

        // Bind the ConnectionsOptions section from appsettings.json to the ConnectionOptions class model
        var connectionOptions = new ConnectionOptions();
        builder.Configuration.GetSection(ConnectionOptions.ConnectionStrings).Bind(connectionOptions);

        return connectionOptions;
    }
    

    private static void ConfigureFilePath(HostApplicationBuilder builder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Construct the absolute path to the appsettings.json file
        var appSettingsPath = Path.Combine(baseDirectory, AppSettingsFileName);

        builder.Configuration
            .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
    }
}
