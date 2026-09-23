namespace YoutubeTest.Shared;

public static class LogConstants
{
    public const string LogDirectory = "logs";
    public const string LogFileName = "youtubetest-";
    public const string OutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:j}{NewLine}{Exception}";
}
