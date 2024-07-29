# ConsoleDownloader
A simple lightweight YouTube downloader that works from the console.

It's being made with convenience heavily in mind. As such, you can compile it to a single executable that automatically deals downloading FFmpeg and then deletes it when the task finishes, so only the primary executable is present at all times.

It's also being made with simplicity in mind, so if you are looking for what is essentially a YouTube client with downloading support, I heavily recommend [YouTubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrz.

Compiled binaries are also available from the latest successful action.

- Currently supports two commands:
  - **-t** *url* Downloads the thumbnail from url
  - **-a** *url* Downloads audio from url
  - *url* Downloads audio and video from url, and then combines them
