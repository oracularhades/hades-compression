# What does Hades Compression do?
[FFmpeg](https://ffmpeg.org/) can **significantly** reduce video file sizes, without losing quality. Hades compression acts as a simple UI for [FFmpeg](https://ffmpeg.org/). [FFmpeg](https://ffmpeg.org/) requires you to operate it from your Terminal (Search "Terminal" or "command prompt" on Windows. Search "terminal" on MacOS), and instead of using complex commands such as ```ffmpeg -i output.mp4 -c:v libx264 -crf 18 -preset slow -c:a aac -b:a 256k -map 0:v -map 0:a:0 -map 0:a:1 -map 0:a:2 -map 0:a:3 -map 0:a:4 output3.mp4```, Hades-Compression gives FFmpeg a simple UI, with additional features such as:
- Automatically compress (encode) files. For example, if OBS outputs a recording to ```C:\Users\user\Videos```, Hades-Compression can automatically encode and output it to a folder, such as your Onedrive/Dropbox/Google Drive folder for editors. This saves a lot of money in cloud storage bills, woohoo! 🎉
- Pause FFmpeg encoding and resume it later.
- Pause encoding when streaming or recording in OBS.

# Just how much can FFMPEG compress a file?

Here, a creator compressed a 10.2GB file to 3.79GB, **without losing any quality**. This was a GTA V video, and all vods combined were 1.3TB.

![Screenshot_2_censored](https://github.com/oracularhades/hades-compression/assets/91714073/7929773a-b32e-4ffb-a0d5-b86f815ac8c2)

# Before you start.
These are test builds. There will be bugs. I'm not aware of any bugs that lose data (and the program **never** deletes files without asking, only a popup asking if you want to remove uncompressed files), however, the program might hang, lag in the UI or bug in certain operations (such as updating the status of a compression from "paused" to "compressing" in the UI, despite it compressing in the background). You can see all known bugs in the [release](https://github.com/oracularhades/hades-compression/releases) information. Use this program at your own risk, no program is bug-free. I am not liable, including for any damages. Comply with local laws.

Please be sure to check your video is as expected before deleting the uncompressed file. Video encoding is wizardry, and things can go wrong.

I did not build the video compression here, though I'm sure every developer wishes they could take credit for it. All video encoding is done via [FFmpeg](https://ffmpeg.org/). I built the UI and awesome additional features listed above.

For potential contributors: I've barely done any code clean-up, I will go back and optimise/comment the code, but bare with me. These are still test builds.

# How to install
https://github.com/oracularhades/hades-compression/wiki/How-to-install
