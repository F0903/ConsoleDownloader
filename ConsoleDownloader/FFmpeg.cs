﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace YTDownloader
{
    public class FFmpeg : IDisposable
    {
        static FFmpeg()
        {
            isFfmpegEmbedded = !File.Exists(FFmpegLocation);
        }

        const string FFmpegLocation = "./ffmpeg.exe";

        static Assembly? cachedAsm;
        static readonly bool isFfmpegEmbedded = false;

        Process? ffmpegProcess;

        async Task GetEmbeddedFFmpegAsync()
        {
            cachedAsm ??= GetType().Assembly;
            using var stream = cachedAsm.GetManifestResourceStream("ConsoleDownloader.ffmpeg.exe") ?? throw new Exception("Could not retrieve embedded ffmpeg.exe");
            using var fs = File.Create(FFmpegLocation);
            await stream.CopyToAsync(fs);
        }

        public Task ProcessAsync(string media, string outputPath)
        {
            if (Path.EndsInDirectorySeparator(outputPath))
                throw new Exception("Please specify a file path, not a directory.");

            ffmpegProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = FFmpegLocation,
                Arguments = $"-y -i {media} -c:a aac {outputPath}",
                CreateNoWindow = false,
                RedirectStandardOutput = true
            });
            ffmpegProcess.WaitForExit();
            return Task.CompletedTask;
        }

        public async Task MuxAsync(string videoPath, string audioPath, string outputPath)
        {
            if (Path.EndsInDirectorySeparator(outputPath))
                throw new Exception("Please specify a file path, not a directory.");

            if (isFfmpegEmbedded)
                await GetEmbeddedFFmpegAsync();

            ffmpegProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = FFmpegLocation,
                Arguments = $"-y -i {videoPath} -i {audioPath} -c:v copy -c:a aac {outputPath}",
                CreateNoWindow = false,
                RedirectStandardOutput = true
            });
            ffmpegProcess.WaitForExit();
        }

        public void Dispose()
        {
            ffmpegProcess?.Close();
            if (isFfmpegEmbedded)
                File.Delete(FFmpegLocation);
        }
    }
}