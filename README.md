# ConsoleDownloader
A simple lightweight YouTube downloader that works from the console.

It's being made with convenience heavily in mind. As such, you can compile it to a single executable that automatically deals with extracting FFmpeg from itself and then deletes it when the task finishes, so only the primary executable is present at all times.

- Currently supports two commands:
  - **-t** Downloads the thumbnail from url
  - **-a** Downloads audio from url
  - **none** Downloads audio and video from url, and then combines them
