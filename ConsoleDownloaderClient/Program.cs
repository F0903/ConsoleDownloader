using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YTDownloader.Downloaders;

namespace YTDownloaderClient
{
    public static class ArgumentHandler
    {
        public delegate Task AsyncArgHandler(string argValue);

        public static AsyncArgHandler? DefaultHandler { private get; set; }
        public static IDictionary<char, AsyncArgHandler>? Handlers { private get; set; }

        const char ArgSpecifier = '-';

        public static Task HandleAsync(ReadOnlySpan<char> input)
        {
            if (Handlers == null)
                throw new NullReferenceException("Handlers were not initialized.");
            for (int i = 1; i < 2; i++) //TODO: Expand to include strings as args
            {
                if (input[i - 1] != ArgSpecifier)
                    continue;
                if (input[i + 1] != ' ')
                    continue;

                if (Handlers.TryGetValue(input[i], out var f))
                    return f(input[(i + 2)..].ToString());
            }
            return DefaultHandler?.Invoke(input.ToString()) ?? Task.CompletedTask;
        }
    }

    class Program
    {
        static readonly YouTubeDownloader youtube = new YouTubeDownloader();

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
                ReadOnlyMemory<char> input = Console.ReadLine().AsMemory();
                try
                {
                    var t = ArgumentHandler.HandleAsync(input.Span);
                    await t;
                }
                catch (Exception ex)
                {
                    const int msDelay = 5000;
                    Console.WriteLine($"Error: {ex.Message}\nClearing in {msDelay / 1000}s...");
                    await Task.Delay(msDelay);
                }
                Console.Clear();
            }
        }
    }
}
