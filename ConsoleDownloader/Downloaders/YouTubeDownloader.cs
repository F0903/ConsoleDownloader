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

        async Task<Stream> DownloadAudio(StreamManifest manifest)
        {
            var audioInfo = manifest.GetAudioOnly().WithHighestBitrate() ?? throw new Exception("No audio streams were found for the specified url");
            var audioStream = await client.Videos.Streams.GetAsync(audioInfo);
            return audioStream;
        }

        async Task<Stream> DownloadVideo(StreamManifest streamManifest)
        {
            var videoInfo = streamManifest.GetVideoOnly().WithHighestVideoQuality() ?? throw new Exception("No video streams were found for the specified url");
            var videoStream = await client.Videos.Streams.GetAsync(videoInfo);
            return videoStream;
        }

        public async override Task DownloadThumbnailAsync(string url, string saveDir)
        {
            VideoId id = url;
            var video = await client.Videos.GetAsync(id);
            var thumbnailUrl = video.Thumbnails.HighResUrl;

            using WebClient web = new WebClient();
            byte[] data = await web.DownloadDataTaskAsync(thumbnailUrl);
            await File.WriteAllBytesAsync($"{saveDir}{id}.jpg", data);
        }

        public override async Task DownloadAudioOnlyAsync(string url, string saveDir)
        {
            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            using var data = await DownloadAudio(streamManifest);
            await MediaSaver.SaveAsync(MediaType.Audio, id, data);
        }

        public override async Task DownloadCombinedAsync(string url, string saveDir)
        {
            if (!Directory.Exists(saveDir))
                throw new Exception("Specified save directory could not be found.");

            VideoId id = url;
            var streamManifest = await client.Videos.Streams.GetManifestAsync(id);

            using var audio = await DownloadAudio(streamManifest);
            using var video = await DownloadVideo(streamManifest);
            using var aStream = audio;
            using var vStream = video;

            await MediaSaver.SaveAsCombined(($"{id}_video", vStream), ($"{id}_audio", aStream), id);
        }
    }
}
