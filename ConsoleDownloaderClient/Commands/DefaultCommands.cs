using System;
using System.Threading.Tasks;
using ConsoleDownloader.Downloaders.YouTube;
using ConsoleDownloaderClient.CommandFramework;

namespace ConsoleDownloaderClient.Commands;

[CommandGroup]
public static class DefaultCommands
{
    static readonly YouTubeDownloader youtube = new();

    [AsyncCommand("version", "Gets the version number of the app.")]
    public static Task VersionAsync(string[] args)
    {
        Console.Write(Updater.CurrentVersion);
        return Task.CompletedTask;
    }

    [AsyncCommand("help", "Prints commands.")]
    public static Task HelpAsync(string[] args)
    {
        Console.WriteLine("Commands:\n--------");
        var commands = AsyncCommandRegistry.GetCommands();
        foreach (var cmd in commands)
        {
            var cmdAttrValues = cmd.AttributeValues;
            Console.Write($"{cmdAttrValues.Name}\n");
            Console.Write($"Description: {cmdAttrValues.Description}\n");
            if (cmdAttrValues.Aliases is not null) Console.Write($"Aliases: {cmdAttrValues.Aliases.FlattenStrings(", ")}\n");
            if (cmdAttrValues.SwitchDescriptions is not null) Console.Write($"Switches: {cmdAttrValues.SwitchDescriptions.FlattenStrings(", ")}\n");
            Console.Write('\n');
        }
        return Task.CompletedTask;
    }

    public static Task ClearAsync(string[] args)
    {
        Console.Clear();
        return Task.CompletedTask;
    }

    static async Task SetupFFmpeg()
    {
        Console.WriteLine("Downloading FFmpeg...");
        try
        {
            await FFmpeg.EnsureAvailableAsync();
            Console.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading ffmpeg!\n{ex}");
        }

        AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
        {
            FFmpeg.Delete();
        };
    }

    [AsyncCommand("download", "Downloads media.", ["get"],
        [
            "-a Audio only",
            "-t Thumbnail only"
        ])
    ]
    public static async Task DownloadAsync(string[] args)
    {
        if (!FFmpeg.Downloaded)
            await SetupFFmpeg();

        string? url = null;
        bool audioOnly = false;
        bool thumbnailOnly = false;
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-a":
                    audioOnly = true;
                    continue;
                case "-t":
                    thumbnailOnly = true;
                    continue;
                default:
                    url = arg;
                    break;
            }
        }
        if (audioOnly && thumbnailOnly)
            throw new Exception("Cannot use both audio and thumbnail switch at the same time!");
        if (url is null)
            throw new NullReferenceException("No url specified!");

        if (audioOnly)
            await youtube.DownloadAudioOnlyAsync(url, "./");
        else if (thumbnailOnly)
            await youtube.DownloadThumbnailAsync(url, "./");
        else
            await youtube.DownloadCombinedAsync(url, "./");
    }
}
