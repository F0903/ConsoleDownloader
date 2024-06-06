using System;
using System.Threading.Tasks;

using ConsoleDownloader.Downloaders.YouTube;
using ConsoleDownloaderClient;

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


Console.Title = "ConsoleDownloader";

var youtube = new YouTubeDownloader();

AsyncCommandHandler.RegisterCommands(
    new(
        "download",
        "Downloads media.\nOptions:\n-a Audio only\n-t Thumbnail only",
        async (args) =>
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
    ),
    new (
        "version", 
        "Gets the version number of the app.",
        (args) => 
        { 
            Console.Write(VersionControl.CurrentVersion);
            return Task.CompletedTask;
        }
    ),
    new (
        "help", 
        "Prints commands.", 
        (args) =>
        {
            Console.WriteLine("Commands:\n");
            var commands = AsyncCommandHandler.GetCommands();
            foreach (var cmdPair in commands)
            {
                var cmd = cmdPair.Value;
                Console.WriteLine($"{cmd.Name}\n{cmd.Description}");
                Console.WriteLine();
            }
            return Task.CompletedTask;
        }
    ),
    new (
        "clear", 
        "Clears the console.", 
        (args) =>
        {
            Console.Clear();
            return Task.CompletedTask;
        }
    )
);

await VersionControl.CheckUpdateAsync();
await AsyncCommandHandler.ExecuteCommandAsync("help", []);
while (true)
{
    Console.Write("-> ");
    string? input = Console.ReadLine();
    if (input == null)
        continue;
    try
    {
        var splitArgs = input.Split(' ');
        await AsyncCommandHandler.ExecuteCommandAsync(splitArgs[0], splitArgs[1..]);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error!\n{ex.Message}\n");
    }
    Console.WriteLine();
}
