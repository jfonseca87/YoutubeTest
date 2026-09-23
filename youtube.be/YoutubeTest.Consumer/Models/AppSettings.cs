namespace YoutubeTest.Consumer.Models;

public class AppSettings
{
    public string YouTubeBaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";
    public string Token { get; set; } = string.Empty;

    // Example: "input\videos.json" (relative to the Consumer project directory)
    public string InputPath { get; set; } = string.Empty;

    // Example: "output\videos.json" (relative to youtube.be/, outside the Consumer project)
    public string OutputPath { get; set; } = string.Empty;

    public int BatchSize { get; set; } = 50;
}
