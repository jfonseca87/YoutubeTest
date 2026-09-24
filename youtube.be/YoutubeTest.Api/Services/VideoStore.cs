using System.Text.Json;
using Microsoft.Extensions.Options;
using YoutubeTest.Api.Models;
using YoutubeTest.Shared.Extensions;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Api.Services;

public class VideoStore(IOptions<ApiSettings> options, ILogger<VideoStore> logger)
{
    public bool IsLoaded { get; private set; }

    public string? SourcePath { get; private set; }

    public IReadOnlyList<Video> Videos =>
        _videos ?? throw new InvalidOperationException(
            "Video data has not been loaded. Call VideoStore.Load() during application startup before serving requests.");

    private List<Video>? _videos;

    public void Load()
    {
        if (IsLoaded)
            return;

        var configuredPath = options.Value.OutputPath;

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException(
                "Configuration value 'OutputPath' is missing. Set \"OutputPath\" in appsettings.json to the videos JSON file.");
        }

        var sourcePath = configuredPath.ResolveOutsideProjectPath();

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                $"Video data file not found at '{sourcePath}'. Ensure the Consumer has written output/videos.json before starting the API.",
                sourcePath);
        }

        try
        {
            var json = File.ReadAllText(sourcePath);
            var videos = JsonSerializer.Deserialize<List<Video>>(json);

            if (videos is null)
            {
                throw new InvalidOperationException(
                    $"Video data file at '{sourcePath}' deserialized to null. The file must contain a JSON array of videos.");
            }

            _videos = videos;
            SourcePath = sourcePath;
            IsLoaded = true;
            logger.LogInformation("Loaded {VideoCount} videos from {VideoPath}", videos.Count, sourcePath);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Video data file at '{sourcePath}' contains invalid JSON. Regenerate it with the Consumer.",
                ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Video data file at '{sourcePath}' could not be read. Check file permissions and that it is not locked.",
                ex);
        }
    }
}
