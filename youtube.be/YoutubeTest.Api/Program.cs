using Serilog;
using YoutubeTest.Api.Models;
using YoutubeTest.Api.Services;
using YoutubeTest.Shared;
using YoutubeTest.Shared.Extensions;

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
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddOpenApi();
    builder.Services.Configure<ApiSettings>(builder.Configuration);
    builder.Services.AddSingleton<VideoStore>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ViteDev", policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
    });

    var app = builder.Build();

    var videoStore = app.Services.GetRequiredService<VideoStore>();
    videoStore.Load();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors("ViteDev");

    app.MapGet("/api/videos", (VideoStore store, int page = 1, int pageSize = 24) =>
    {
        page = Math.Max(page, 1);
        pageSize = pageSize < 1 ? 24 : Math.Min(pageSize, 100);

        var totalCount = store.Videos.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var items = store.Videos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Results.Ok(new PagedVideosResponse(items, page, pageSize, totalCount, totalPages));
    })
    .WithName("GetVideos")
    .WithTags("Videos");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated with an error");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
