using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient;
public static class VersionControl
{
    static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();
    public static readonly Version CurrentVersion = currentAssembly.GetName().Version ?? throw new NullReferenceException("Version not defined!");

    const string apiEndpoint = "https://api.github.com/repos/F0903/ConsoleDownloader/releases";

    static async Task<(Version, string)> GetRemoteVersionAsync()
    {
        Console.WriteLine("Checking for new version...");
        var http = new HttpClient();
        var asmName = currentAssembly.FullName!;
        http.DefaultRequestHeaders.UserAgent.Add(new("ConsoleDownloader", CurrentVersion.ToString()));
        var response = await http.GetAsync(apiEndpoint);
        response.EnsureSuccessStatusCode();
        using var stream = response.Content.ReadAsStream();
        using var json = await JsonDocument.ParseAsync(stream);
        var root = json.RootElement[0];
        var tagName = root.GetProperty("tag_name").GetString()!;
        var htmlUrl = root.GetProperty("html_url").GetString()!;
        var assetsArray = root.GetProperty("assets").EnumerateArray();
        //TODO: return download url of exe
        return (new(tagName), htmlUrl);
    }

    public static async Task CheckUpdateAsync()
    {
        try
        {
            var (remoteVersion, newVersionUrl) = await GetRemoteVersionAsync();
            if (remoteVersion > CurrentVersion)
            {
                Console.WriteLine("A newer version is available!\n Url: {}");
            }
        }
        catch {
            Console.WriteLine("Unable to get release info.");
        }
    } 
}
