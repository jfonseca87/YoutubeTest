using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YoutubeTest.Consumer.Models;

namespace YoutubeTest.Consumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AppSettings>()
            .Bind(configuration)
            .Validate(s => !string.IsNullOrWhiteSpace(s.Token),
                "Secret 'Token' is not configured. Run: dotnet user-secrets set \"Token\" \"<your-token>\"")
            .Validate(s => !string.IsNullOrWhiteSpace(s.InputPath),
                "Secret 'InputPath' is not configured. Run: dotnet user-secrets set \"InputPath\" \"<input-json-path>\"")
            .Validate(s => !string.IsNullOrWhiteSpace(s.OutputPath),
                "Secret 'OutputPath' is not configured. Run: dotnet user-secrets set \"OutputPath\" \"<output-json-path>\"")
            .ValidateOnStart();

        return services;
    }
}
