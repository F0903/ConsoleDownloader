using System.IO;
using System.Threading.Tasks;

namespace ConsoleDownloaderClient
{
    public enum MediaType
    {
        Audio,
        Video
    }

    public static class MediaSaver
    {
        public enum AudioFormat
        {
            mp3
        }

        public enum VideoFormat
        {
            mp4
        }

        public static AudioFormat AudioSaveFormat { get; set; }

        public static VideoFormat VideoSaveFormat { get; set; }

        static string GetPath(string name, MediaType type) =>
            Path.ChangeExtension(Path.Combine("./", name), type == MediaType.Audio ? AudioSaveFormat.ToString() : VideoSaveFormat.ToString());

        public static async Task<string> SaveAsync(MediaType type, string name, Stream data, string inFormat, bool doNotProcess = false)
        {
            var outPath = GetPath(name, type);
            var interPath = GetPath($"temp-{name}", type);

            using var fs = File.Create(interPath);
            await data.CopyToAsync(fs);
            fs.Close();

            if (!doNotProcess && $".{inFormat}" != Path.GetExtension(interPath))
            {
                using var ffmpeg = new FFmpeg();
                await ffmpeg.ProcessAsync(interPath, outPath);
                File.Delete(interPath);
            }
            else
            {
                File.Move(interPath, outPath);
            }

            return outPath;
        }

        public static Task<string[]> SaveAllAsync(params (MediaType type, string name, Stream stream, string inFormat)[] toSave)
        {
            int i = 0;
            string[] paths = new string[toSave.Length];
            Parallel.ForEach(toSave, x =>
            {
                paths[i++] = SaveAsync(x.type, x.name, x.stream, x.inFormat, true).Result;
            });
            return Task.FromResult(paths);
        }

        public static async Task SaveAsCombined((string title, Stream data, string inFormat) audio, (string title, Stream data, string inFormat) video, string finalName)
        {
            var paths = await SaveAllAsync((MediaType.Audio, audio.title, audio.data, audio.inFormat), (MediaType.Video, video.title, video.data, audio.inFormat));
            using var ffmpeg = new FFmpeg();
            await ffmpeg.MuxAsync(paths[1], paths[0], GetPath(finalName, MediaType.Video));
            for (int i = 0; i < paths.Length; i++)
                File.Delete(paths[i]);
        }
    }
}
