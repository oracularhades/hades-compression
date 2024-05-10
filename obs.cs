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
            Objects.advanced_settings advanced_settings = await advanced.get();

            // advanced_settings.obs_address can only contain certain characters, you should never pass uncontrolled data like this.
            string arguments = $"--address {advanced_settings.obs_address}";
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
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rust-obs.exe"),
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

            await sw.WriteLineAsync(advanced_settings.obs_password);

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

                return new Objects.obs_output {
                    failed_to_connect = true
                };
            }

            ffmpeg_cmd.WaitForExit();

            Objects.obs_output obs_output = JsonConvert.DeserializeObject<Objects.obs_output>(output);

            Debug.WriteLine($"FINISHING OBS {output}");

            return obs_output;
        }
        public static async Task<bool> block_because_of_status()
        {
            bool output = false;

            Objects.settings settings = await Settings.get();

            Objects.obs_output obs_output = await Obs.get_obs_status(true, true);
            if (obs_output != null && obs_output.stream != null && obs_output.stream.outputActive == true) {
                output = true;

                if (settings.encode_while_streaming == true)
                {
                    return false;
                }
            } else if (obs_output != null && obs_output.recording != null && obs_output.recording.outputActive == true) {
                output = true;

                if (settings.encode_while_recording == true)
                {
                    return false;
                }
            } else if (obs_output != null && obs_output.recording != null && obs_output.recording.outputPaused == true) {
                output = true;

                if (settings.encode_while_recording == true)
                {
                    return false;
                }
            }

            return output;
        }
    }
}
