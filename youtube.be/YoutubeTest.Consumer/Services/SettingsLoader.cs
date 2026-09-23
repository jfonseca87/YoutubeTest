using Microsoft.Extensions.Configuration;
using YoutubeTest.Consumer.Models;

namespace YoutubeTest.Consumer.Services;

public static class SettingsLoader
{
    public static AppSettings Load()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(typeof(SettingsLoader).Assembly)
            .Build();

        var settings = config.Get<AppSettings>() ?? new AppSettings();

        if (string.IsNullOrWhiteSpace(settings.Token))
            throw new InvalidOperationException("Secret 'Token' no configurado. Ejecuta: dotnet user-secrets set \"Token\" \"<tu-token>\"");
        if (string.IsNullOrWhiteSpace(settings.InputPath))
            throw new InvalidOperationException("Secret 'InputPath' no configurado. Ejecuta: dotnet user-secrets set \"InputPath\" \"<ruta-json-input>\"");
        if (string.IsNullOrWhiteSpace(settings.OutputPath))
            throw new InvalidOperationException("Secret 'OutputPath' no configurado. Ejecuta: dotnet user-secrets set \"OutputPath\" \"<ruta-json-output>\"");

        return settings;
    }
}
