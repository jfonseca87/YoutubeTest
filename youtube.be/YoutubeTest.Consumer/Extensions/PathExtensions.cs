namespace YoutubeTest.Consumer.Extensions;

public static class PathExtensions
{
    public static string ResolveProjectPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(GetProjectDirectory(), path));
    }

    public static string ResolveOutsideProjectPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        var projectDirectory = GetProjectDirectory();
        var parentDirectory = Path.GetDirectoryName(projectDirectory);

        if (string.IsNullOrEmpty(parentDirectory))
            parentDirectory = projectDirectory;

        return Path.GetFullPath(Path.Combine(parentDirectory, path));
    }

    private static string GetProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (directory.GetFiles("*.csproj").Length > 0)
                return directory.FullName;

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
