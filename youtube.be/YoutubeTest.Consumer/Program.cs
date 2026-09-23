using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using YoutubeTest.Consumer.Extensions;
using YoutubeTest.Consumer.Handlers;
using YoutubeTest.Consumer.Models;
using YoutubeTest.Consumer.Services;
using YoutubeTest.Shared;

var logDirectory = LogConstants.LogDirectory.ResolveOutsideProjectPath();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: Path.Combine(logDirectory, LogConstants.LogFileName + ".txt"),
        outputTemplate: LogConstants.OutputTemplate,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .WriteTo.Console(outputTemplate: LogConstants.OutputTemplate)
    .CreateLogger();

try
{
    var configuration = new ConfigurationBuilder()
        .AddUserSecrets(typeof(Program).Assembly)
        .Build();

    var services = new ServiceCollection();

    services.AddAppSettings(configuration);

    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSerilog();
    });

    services.AddHttpClient<YouTubeFetcher>((sp, client) =>
    {
        var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
        client.BaseAddress = new Uri(settings.YouTubeBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("YoutubeTest.Consumer/1.0");
    })
    .AddHttpMessageHandler(sp =>
        new YouTubeAuthHandler(sp.GetRequiredService<IOptions<AppSettings>>().Value.Token))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });

    services.AddTransient<JsonOutputWriter>();
    services.AddTransient<BatchProcessor>();

    await using var provider = services.BuildServiceProvider();

    var processor = provider.GetRequiredService<BatchProcessor>();
    await processor.ProcessAsync();
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
