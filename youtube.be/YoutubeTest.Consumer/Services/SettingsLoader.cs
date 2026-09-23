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
            throw new InvalidOperationException("Secret 'Token' is not configured. Run: dotnet user-secrets set \"Token\" \"<your-token>\"");
        if (string.IsNullOrWhiteSpace(settings.InputPath))
            throw new InvalidOperationException("Secret 'InputPath' is not configured. Run: dotnet user-secrets set \"InputPath\" \"<input-json-path>\"");
        if (string.IsNullOrWhiteSpace(settings.OutputPath))
            throw new InvalidOperationException("Secret 'OutputPath' is not configured. Run: dotnet user-secrets set \"OutputPath\" \"<output-json-path>\"");

        return settings;
    }
}
