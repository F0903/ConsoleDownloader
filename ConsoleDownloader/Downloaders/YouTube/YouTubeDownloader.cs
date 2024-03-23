using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace ConsoleDownloader.Downloaders.YouTube
{
    public class YouTubeDownloader : Downloader
    {
        private readonly YoutubeClient client = new();

        async Task<(Stream data, string format)> GetBestAudioStreamAsync(StreamManifest manifest)
        {
            Console.WriteLine("Getting audio stream...");
            var audioInfo = manifest.GetAudioOnlyStreams().GetWithHighestBitrate() ?? throw new Exception("No audio streams were found for the specified url");
            var audioStream = await client.Videos.Streams.GetAsync(audioInfo);
            return (audioStream, audioInfo.Container.Name);
        }

        async Task<(Stream data, string format)> GetBestVideoStreamAsync(StreamManifest streamManifest)
        {
            Console.WriteLine("Getting video stream...");
            var videoInfo = streamManifest.GetVideoOnlyStreams().GetWithHighestVideoQuality() ?? throw new Exception("No video streams were found for the specified url");
            var videoStream = await client.Videos.Streams.GetAsync(videoInfo);
            return (videoStream, videoInfo.Container.Name);
        }

        public async override Task DownloadThumbnailAsync(string url, string saveDir)
        {
            Console.WriteLine("Getting video info...");
            VideoId id = url;
            var video = await client.Videos.GetAsync(id);
            var thumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url;

            Console.WriteLine("Downloading thumbnail...");
            using HttpClient web = new();
            using var data = await web.GetStreamAsync(thumbnailUrl);
            using var thumbnailFile = File.OpenWrite($"{saveDir}{id}.jpg");
            await data.CopyToAsync(thumbnailFile);
        }

        public override async Task DownloadAudioOnlyAsync(string url, string saveDir)
        {
            Console.WriteLine("Getting audio only stream...");
            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            var (data, format) = await GetBestAudioStreamAsync(streamManifest);
            using (data)
            {
                await MediaSaver.SaveAsync(MediaType.Audio, id, data, format);
            }
        }

        public override async Task DownloadCombinedAsync(string url, string saveDir)
        {
            if (!Directory.Exists(saveDir))
                throw new Exception("Specified save directory could not be found.");

            Console.WriteLine("Getting video manifest...");
            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            var (aData, aFormat) = await GetBestAudioStreamAsync(streamManifest);
            var (vData, vFormat) = await GetBestVideoStreamAsync(streamManifest);

            using (aData)
            using (vData)
            {
                await MediaSaver.SaveAsCombined(($"{id}_audio", aData, aFormat), ($"{id}_video", vData, vFormat), id);
            }
        }
    }
}
