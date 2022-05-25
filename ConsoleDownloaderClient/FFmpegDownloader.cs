using System;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient;

public static class FFmpeg
{
    const string WindowsUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v4.4.1/ffmpeg-4.4.1-win-64.zip";

    const string path = "ffmpeg.exe";

    public static async Task DownloadAsync()
    {
        using var http = new HttpClient();
        using var result = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, WindowsUrl));
        result.EnsureSuccessStatusCode();
        using var zipped = result.Content.ReadAsStream();
        using var zip = new ZipArchive(zipped);
        zip.GetEntry("ffmpeg.exe")!.ExtractToFile(path);
    }

    public static void Delete()
    {
        File.Delete(path);
    }
}
