using System;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient;

public static class FFmpeg
{
    const string WindowsUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v6.1/ffmpeg-6.1-win-64.zip";

    public const string Path = "ffmpeg.exe";

    public static bool Downloaded { get; set; } = false;

    public static async Task EnsureAvailableAsync()
    {
        if (File.Exists(Path))
            return;

        using var http = new HttpClient();
        using var result = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, WindowsUrl));
        result.EnsureSuccessStatusCode();
        using var zipped = result.Content.ReadAsStream();
        using var zip = new ZipArchive(zipped);
        zip.GetEntry("ffmpeg.exe")!.ExtractToFile(Path, true);
        Downloaded = true;
    }

    public static void Delete()
    {
        File.Delete(Path);
        Downloaded = false;
    }
}
