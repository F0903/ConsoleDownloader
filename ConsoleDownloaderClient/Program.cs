using System;
using System.Threading.Tasks;
using ConsoleDownloaderClient.CommandFramework;

namespace ConsoleDownloaderClient;
public static class Program
{
    static async Task HandleUpdatesAsync(string? oldAppPath)
    {
        try
        {
            if (oldAppPath is not null)
                await Updater.FinishUpdate(oldAppPath);
            else
                await Updater.CheckUpdateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static async Task Setup(string[] args)
    {
        Console.Title = "ConsoleDownloader";

        string? oldAppPath = null;
        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "--finish-update":
                        oldAppPath = args[i + 1];
                        break;
                    case "--version":
                        Console.Write(Updater.CurrentVersion);
                        return;
                    default:
                        break;
                }
            }
        }

        await HandleUpdatesAsync(oldAppPath);
    }

    static async Task Main(string[] args)
    {
        await Setup(args);

        await AsyncCommandRegistry.ExecuteCommandAsync("help", []);
        while (true)
        {
            Console.Write("-> ");
            string? input = Console.ReadLine();
            if (input == null)
                continue;
            try
            {
                var splitArgs = input.Split(' ');
                await AsyncCommandRegistry.ExecuteCommandAsync(splitArgs[0], splitArgs[1..]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error!\n{ex.Message}\n");
            }
            Console.WriteLine();
        }
    }
}