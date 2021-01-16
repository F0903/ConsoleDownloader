using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YTDownloader.Downloaders
{
    public class YouTubeDownloader : Downloader
    {
        private readonly YoutubeClient client = new YoutubeClient();

        async Task<(Stream data, string format)> DownloadAudio(StreamManifest manifest)
        {
            Console.WriteLine("Downloading audio...");
            var audioInfo = manifest.GetAudioOnly().WithHighestBitrate() ?? throw new Exception("No audio streams were found for the specified url");
            var audioStream = await client.Videos.Streams.GetAsync(audioInfo);
            return (audioStream, audioInfo.Container.Name);
        }

        async Task<(Stream data, string format)> DownloadVideo(StreamManifest streamManifest)
        {
            Console.WriteLine("Downloading video...");
            var videoInfo = streamManifest.GetVideoOnly().WithHighestVideoQuality() ?? throw new Exception("No video streams were found for the specified url");
            var videoStream = await client.Videos.Streams.GetAsync(videoInfo);
            return (videoStream, videoInfo.Container.Name);
        }

        public async override Task DownloadThumbnailAsync(string url, string saveDir)
        {
            VideoId id = url;
            var video = await client.Videos.GetAsync(id);
            var thumbnailUrl = video.Thumbnails.HighResUrl;

            Console.WriteLine("Downloading thumbnail...");
            using WebClient web = new WebClient();
            byte[] data = await web.DownloadDataTaskAsync(thumbnailUrl);
            await File.WriteAllBytesAsync($"{saveDir}{id}.jpg", data);
        }

        public override async Task DownloadAudioOnlyAsync(string url, string saveDir)
        {
            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            var (data, format) = await DownloadAudio(streamManifest);
            using (data)
            {
                await MediaSaver.SaveAsync(MediaType.Audio, id, data, format);
            }
        }

        public override async Task DownloadCombinedAsync(string url, string saveDir)
        {
            if (!Directory.Exists(saveDir))
                throw new Exception("Specified save directory could not be found.");

            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            var (aData, aFormat) = await DownloadAudio(streamManifest);
            var (vData, vFormat) = await DownloadVideo(streamManifest);

            using (aData)
            using (vData)
            {
                await MediaSaver.SaveAsCombined(($"{id}_audio", aData, aFormat), ($"{id}_video", vData, vFormat), id);
            }
        }
    }
}
