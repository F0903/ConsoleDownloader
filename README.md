# ConsoleDownloader
A simple lightweight YouTube downloader that works from the console.

It's being made with convenience heavily in mind. As such, you can compile it to a single executable that automatically deals with extracting FFmpeg from itself and then deletes it when the task finishes, so only the primary executable is present at all times.

It's also being made with simplicity in mind, so if you are looking for what is essentially a YouTube client with downloading support, I heavily recommend [YouTubeDownloader](https://github.com/Tyrrrz/YoutubeDownloader) by Tyrrz.

- Currently supports two commands:
  - **-t** Downloads the thumbnail from url
  - **-a** Downloads audio from url
  - **none** Downloads audio and video from url, and then combines them
