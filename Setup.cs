using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HadesCompression
{
    public class Setup
    {
        public static async void check()
        {
            if (Process.GetProcessesByName("rust-obs.exe").Length == 0)
            {
                rust_obs();
            }

            if (Process.GetProcessesByName("ffmpeg").Length == 0 || Process.GetProcessesByName("ffmpeg").Length == 0 || Process.GetProcessesByName("ffplay").Length == 0)
            {
                ffmpeg();
            }
        }
        public static void rust_obs()
        {
            string rust_obs_directory = @$"{FileSystem.AppDataDirectory}\rust-obs";
            Directory.CreateDirectory(rust_obs_directory);
            
            string rust_obs_executable = @$"{rust_obs_directory}\rust-obs.exe";

            Files.download_file("https://github.com/oracularhades/hades-compression/releases/latest/rust-obs.exe", rust_obs_executable);
            Files.add_to_path(rust_obs_executable);
        }
        public static void ffmpeg()
        {
            string ffmpeg_directory = @$"{FileSystem.AppDataDirectory}\ffmpeg";
            Directory.CreateDirectory(ffmpeg_directory);

            string ffmpeg_zip = @$"{FileSystem.AppDataDirectory}\ffmpeg\ffmpeg-master-latest-win64-gpl.zip";

            Files.download_file("https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip", ffmpeg_zip);
            Files.unzip_file(ffmpeg_zip, ffmpeg_directory);
            Files.add_to_path(ffmpeg_directory);
        }
    }
}
