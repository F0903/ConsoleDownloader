using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ConsoleDownloaderClient;
public static class Updater
{
    static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();
    static readonly string asmName = currentAssembly.GetName().Name!;
    public static readonly Version CurrentVersion = currentAssembly.GetName().Version ?? throw new NullReferenceException("Version not defined!");

    const string apiEndpoint = "https://api.github.com/repos/F0903/ConsoleDownloader/releases";

    static async Task<(Version, string)> GetRemoteVersionAsync(HttpClient http)
    {
        Console.WriteLine("Checking for new version...");
        var response = await http.GetAsync(apiEndpoint);
        response.EnsureSuccessStatusCode();

        using var stream = response.Content.ReadAsStream();
        using var json = await JsonDocument.ParseAsync(stream);

        var root = json.RootElement[0];
        var tagName = root.GetProperty("tag_name").GetString()!;
        var htmlUrl = root.GetProperty("html_url").GetString()!;
        var assetsArray = root.GetProperty("assets").EnumerateArray();

        var elem = assetsArray.First(x => x.TryGetProperty("name", out var name) && name.GetString() == $"{asmName}.exe");
        var downloadUrl = elem.GetProperty("browser_download_url").GetString() ?? throw new NullReferenceException("Download url of new version was null!");
        return (new(tagName), downloadUrl);
    }

    public static async Task CheckUpdateAsync()
    {
        try
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.Add(new(asmName, CurrentVersion.ToString()));
            var (remoteVersion, newVersionUrl) = await GetRemoteVersionAsync(http);
            if (remoteVersion <= CurrentVersion)
            {
                Console.WriteLine("No new version found.\n");
                return;
            }

            Console.WriteLine("A newer version is available!\nWould you like to download it? [y/N]");
            var input = Console.ReadLine();
            if (input is null || input.ToLower() is "n" or "no")
            {
                Console.WriteLine("New version ignored.");
                return;
            }
            else if (input.ToLower() is "y" or "yes")
            {
                await DownloadNewVersion(http, remoteVersion, newVersionUrl);
            }
        }
        catch
        {
            Console.WriteLine("Unable to get release info.");
        }
    }

    static async Task DownloadNewVersion(HttpClient http, Version newVer, string url)
    {
        var fileName = $"{asmName}-{newVer}.exe";
        {
            Console.WriteLine("Downloading update...");
            using var stream = await http.GetStreamAsync(url);
            using var file = File.Create(fileName);
            await stream.CopyToAsync(file);
        }
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = $"--finish-update \"{asmName}.exe\"",
            UseShellExecute = true,
        });
        Environment.Exit(0);
    }

    public static async Task FinishUpdate(string oldPath)
    {
        const int ATTEMPTS = 5;
        for (int i = 0; i < ATTEMPTS; i++)
        {
            try
            {
                File.Delete(oldPath);
                Console.WriteLine("Update successful!\n");
                return;
            }
            catch (AccessViolationException)
            {
                await Task.Delay(500);
                continue;
            }
        }
        throw new Exception("Could not delete old version!");
    }
}
