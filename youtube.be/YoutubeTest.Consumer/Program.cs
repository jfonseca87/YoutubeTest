using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YoutubeTest.Consumer.Models;
using YoutubeTest.Consumer.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine("logs", "youtubetest-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var settings = SettingsLoader.Load();

    var services = new ServiceCollection();

    services.AddSingleton(settings);

    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSerilog();
    });

    services.AddHttpClient<YouTubeFetcher>((sp, client) =>
    {
        var config = sp.GetRequiredService<AppSettings>();
        client.BaseAddress = new Uri(config.YouTubeBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("YoutubeTest.Consumer/1.0");
    })
    .AddHttpMessageHandler(sp => new YouTubeAuthHandler(sp.GetRequiredService<AppSettings>().Token))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });

    services.AddTransient<JsonOutputWriter>();
    services.AddTransient<BatchProcessor>();

    await using var provider = services.BuildServiceProvider();

    var processor = provider.GetRequiredService<BatchProcessor>();

    await processor.ProcessAsync(settings.InputPath, settings.OutputPath);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Consumer terminated with an error");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
