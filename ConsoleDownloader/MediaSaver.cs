using System.IO;
using System.Threading.Tasks;

namespace YTDownloader
{
    public enum MediaType
    {
        Audio,
        Video,
        Raw
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

        static async Task WriteToDiskAsync(string path, Stream data)
        {
            using var fStream = File.Create(path);
            await data.CopyToAsync(fStream);
        }

        public static Task SaveAsync(MediaType type, string name, Stream data)
        {
            var path = GetPath(name, type);
            return WriteToDiskAsync(path, data);
        }

        public static async Task<string[]> SaveAllAsync(params (MediaType type, string name, Stream stream)[] toSave)
        {
            int i = 0;
            string[] paths = new string[toSave.Length];
            foreach (var (type, name, stream) in toSave)
            {
                var path = GetPath(name, type);
                paths[i++] = path;
                await WriteToDiskAsync(path, stream);
            }
            return paths;
        }

        public static async Task SaveAsCombined((string title, Stream data) audio, (string title, Stream data) video, string finalName)
        {
            var paths = await SaveAllAsync((MediaType.Raw, audio.title, audio.data), (MediaType.Raw, video.title, video.data));
            using var ffmpeg = new FFmpeg();
            await ffmpeg.MuxAsync(paths[1], paths[0], GetPath(finalName, MediaType.Video));
            for (int i = 0; i < paths.Length; i++)
                File.Delete(paths[i]);
        }
    }
}
