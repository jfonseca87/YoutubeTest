using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
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

    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSerilog();
    });

    services.AddHttpClient<YouTubeFetcher>((sp, client) =>
    {
        client.BaseAddress = new Uri(settings.YouTubeBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("YoutubeTest.Consumer/1.0");
    }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });

    services.AddTransient(sp =>
    {
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient(nameof(YouTubeFetcher));
        var logger = sp.GetRequiredService<ILogger<YouTubeFetcher>>();
        return new YouTubeFetcher(httpClient, logger, settings.Token);
    });

    services.AddTransient<JsonOutputWriter>();
    services.AddTransient(sp =>
    {
        var fetcher = sp.GetRequiredService<YouTubeFetcher>();
        var writer = sp.GetRequiredService<JsonOutputWriter>();
        var logger = sp.GetRequiredService<ILogger<BatchProcessor>>();
        return new BatchProcessor(fetcher, writer, logger, settings.BatchSize);
    });

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
