namespace YoutubeTest.Consumer.Models;

public class AppSettings
{
    public string YouTubeBaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";
    public string Token { get; set; } = string.Empty;
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int BatchSize { get; set; } = 50;
}
