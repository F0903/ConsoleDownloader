using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ConsoleDownloaderClient.Downloaders.YouTube;

namespace ConsoleDownloaderClient
{
    class Program
    {
        static readonly YouTubeDownloader youtube = new();

        static async Task Main()
        {
            ArgumentHandler.DefaultHandler = input => youtube.DownloadCombinedAsync(input, "./");
            ArgumentHandler.Handlers = new Dictionary<char, ArgumentHandler.AsyncArgHandler>
            {
                {'a', input => youtube.DownloadAudioOnlyAsync(input, "./") },
                {'t', input => youtube.DownloadThumbnailAsync(input, "./") }
            };

            Console.Title = "ConsoleDownloader";

            while (true)
            {
                Console.Write("-> ");
                string? input = Console.ReadLine();
                if (input == null)
                    continue;
                try
                {
                    await ArgumentHandler.HandleAsync(input.AsSpan());
                    Console.WriteLine("\nDone!");
                    await Task.Delay(1500);
                }
                catch (Exception ex)
                {
                    const int msDelay = 7000;
                    Console.WriteLine($"Error: {ex.Message}\nClearing in {msDelay / 1000}s...");
                    await Task.Delay(msDelay);
                }
                Console.Clear();
            }
        }
    }
}
