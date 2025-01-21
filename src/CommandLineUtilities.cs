public static class CommandLineUtilities
{
    public static string? GetFilesDirectory()
    {
        var args = Environment.GetCommandLineArgs();
        var directoryFlagPosition = Array.IndexOf(args, "--directory");
        var directory = args.Skip(directoryFlagPosition + 1).FirstOrDefault();
        Console.WriteLine(directory is null ? "Files Directory Not Set." : $"Files Directory Set To: {directory}");
        return directory;
    }
}