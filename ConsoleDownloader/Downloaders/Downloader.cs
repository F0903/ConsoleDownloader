using System.Threading.Tasks;

namespace YTDownloader.Downloaders
{
    public abstract class Downloader
    {
        public abstract Task DownloadThumbnailAsync(string url, string saveDir);

        public abstract Task DownloadCombinedAsync(string url, string saveDir);

        public abstract Task DownloadAudioOnlyAsync(string url, string saveDir);
    }
}
