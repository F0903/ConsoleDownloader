using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsoleDownloader
{
    public static class FFmpeg
    {
        const string FFmpegLocation = "./ffmpeg.exe";

        public static Task ProcessAsync(string inputPath, string outputPath)
        {
            if (Path.EndsInDirectorySeparator(outputPath))
                throw new Exception("Please specify a file path, not a directory.");

            using var ffmpegProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = FFmpegLocation,
                Arguments = $"-y -i {inputPath} {outputPath}",
                CreateNoWindow = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            });
            if (ffmpegProcess == null)
                throw new NullReferenceException("Could not start FFmpeg. Please make sure you have ffmpeg.exe in the root directory.");

            ffmpegProcess.WaitForExit();
            return Task.CompletedTask;
        }

        public static Task MuxAsync(string videoPath, string audioPath, string outputPath)
        {
            if (Path.EndsInDirectorySeparator(outputPath))
                throw new Exception("Please specify a file path, not a directory.");

            using var ffmpegProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = FFmpegLocation,
                Arguments = $"-y -i {videoPath} -i {audioPath} -c:v copy -c:a aac {outputPath}",
                CreateNoWindow = false,
                RedirectStandardOutput = true
            });
            if (ffmpegProcess == null)
                throw new NullReferenceException("Could not start FFmpeg. Please make sure you have ffmpeg.exe in the root directory.");

            ffmpegProcess.WaitForExit();
            return Task.CompletedTask;
        }
    }
}
