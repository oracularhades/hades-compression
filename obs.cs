using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HadesCompression
{
    public class Obs
    {
        public static async Task<Objects.obs_output> get_obs_status(bool stream, bool recording)
        {
            Debug.WriteLine("STARTED OBS");
            string arguments = "";
            if (stream == true) {
                arguments = arguments+" --streaming";
            }
            if (recording == true) {
                arguments = arguments+" --recording";
            }

            Process ffmpeg_cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "./rust-obs/target/release/rust-obs.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            ffmpeg_cmd.Start();

            StreamWriter sw = ffmpeg_cmd.StandardInput;
            StreamReader sr = ffmpeg_cmd.StandardOutput;

            await sw.WriteLineAsync("trexKa19WLoLvdKO");

            string output = null;

            string process_output = null;
            while ((process_output = ffmpeg_cmd.StandardOutput.ReadLine()) != null)
            {
                if (process_output.StartsWith("OUTPUT: ")) {
                    output = process_output.Replace("OUTPUT: ", "");
                }
            }

            if (output == null) {
                // TODO: Display this as a notice to the user.
                Debug.WriteLine("Couldn't reach OBS.");

                return new Objects.obs_output {};
            }

            ffmpeg_cmd.WaitForExit();

            Objects.obs_output obs_output = JsonConvert.DeserializeObject<Objects.obs_output>(output);

            Debug.WriteLine($"FINISHING OBS {output}");

            return obs_output;
        }
    }
}
