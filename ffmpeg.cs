using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HadesCompression
{
    public class ffmpeg
    {
        public static Dictionary<string, Process> ffmpeg_processes = new Dictionary<string, Process>();
        public static Dictionary<string, string> ffmpeg_progress = new Dictionary<string, string>();
        public static Dictionary<string, string> ffmpeg_original_files = new Dictionary<string, string>();
        public static Dictionary<string, string> ffprobe_video_length = new Dictionary<string, string>();
        // TODO: Check all outputs were successful, and return an error if they weren't.
        public static Objects.ffprobevideoinfo ffprobe(string file_path)
        {
            Process ffprobe_cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)), "ffprobe.exe"),
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{file_path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
            };

            ffprobe_cmd.Start();

            // To read the output (if any)
            string output = ffprobe_cmd.StandardOutput.ReadToEnd();

            // To read the error (if any)
            string error = ffprobe_cmd.StandardError.ReadToEnd();

            ffprobe_cmd.WaitForExit();

            // After the process has exited, you can get the status code
            int status_code = ffprobe_cmd.ExitCode;

            // Output the results
            Debug.WriteLine($"Exit Code: {status_code}");
            Debug.WriteLine($"Output: {output}");
            Debug.WriteLine($"Error: {error}");

            ffprobe_cmd.WaitForExit();

            Objects.ffprobevideoinfo ffprobe_output = JsonConvert.DeserializeObject<Objects.ffprobevideoinfo>(output);

            ffprobe_video_length.Remove(file_path);
            ffprobe_video_length.Add(file_path, ffmpeg.seconds_to_formatted(double.Parse(ffprobe_output.format.duration)));

            return ffprobe_output;
        }
        public static string seconds_to_formatted(double input_seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(input_seconds);

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;
            int milliseconds = timeSpan.Milliseconds;

            string formattedTime = $"{hours:00}:{minutes:00}:{seconds:00}.{milliseconds:000}";

            return formattedTime;
        }

        public static Objects.encoding_config get_encoding_config_from_ffprobe(Objects.ffprobevideoinfo ffprobe_output, string output_file)
        {
            Objects.ffprobevideoinfostream video_stream = null;

            List<int> audio_tracks = new List<int>();

            int audio_stream_count = -1;
            foreach (Objects.ffprobevideoinfostream stream in ffprobe_output.streams) {
                if (stream.codec_type == "video") {
                    video_stream = stream;
                }
                if (stream.codec_type == "audio") {
                    audio_stream_count++;
                    audio_tracks.Add(audio_stream_count);
                }
            }

            int quality = video_stream.coded_height;
            string vcodec = video_stream.codec_name;
            string max_rate = null;
            int crf = 18;
            string preset = "medium";

            return new Objects.encoding_config {
                input_file = ffprobe_output.format.filename,
                output_file = output_file,
                quality = quality,
                vcodec = vcodec,
                max_rate = max_rate,
                crf = crf,
                preset = preset,
                audio_tracks = audio_tracks
            };
        }
        
        public static async Task<string> get_arguments(Objects.encoding_config encoding_config, string pick_up_after)
        {
            try {
                string arguments = $"-i \"{Files.is_valid_path(encoding_config.input_file)}\" -map 0:v -c:a aac";

                Objects.settings settings = await Settings.get();

                int cpu_limit_percent = settings.cpu_limit;
                if (cpu_limit_percent != 0)
                {
                    int cpu_threads = Environment.ProcessorCount;
                    double percent_to_threads = cpu_threads * (cpu_limit_percent / 100.0);
                    arguments = arguments + $" -threads {percent_to_threads}";
                }

                int memory_limit_percent = settings.memory_limit;
                if (memory_limit_percent != null)
                {
                    int memory_bytes = System.Runtime.InteropServices.Marshal.SizeOf(typeof(double));
                    double percent_to_bytes = memory_bytes * (memory_limit_percent / 100.0);
                    arguments = arguments + $" -bufsize {percent_to_bytes}";
                }

                if (encoding_config.quality > 0)
                {
                    arguments = arguments + $" -vf \"scale=-2:{encoding_config.quality}\"";
                }

                if (encoding_config.vcodec != null)
                {
                    arguments = arguments + $" -c:v {encoding_config.vcodec}";
                }

                if (encoding_config.max_rate != null)
                {
                    arguments = arguments + $" -maxrate {encoding_config.max_rate}";
                }

                if (encoding_config.crf != 0)
                {
                    arguments = arguments + $" -crf {encoding_config.crf}";
                }

                if (encoding_config.preset != null)
                {
                    arguments = arguments + $" -preset {encoding_config.preset}";
                }
                if (pick_up_after != null)
                {
                    arguments = arguments + $" -ss {pick_up_after}";
                }

                foreach (int audio_stream in encoding_config.audio_tracks) {
                    arguments = arguments + $" -map 0:a:{audio_stream}";
                }

                arguments = arguments + $" \"{Files.is_valid_path(encoding_config.output_file)}\"";

                return arguments;
            } catch (Exception e)
            {
                Debug.WriteLine("ERROR GETTING FFMPEG ARGUMENTS "+e);
                return null;
            }
        }

        public static async Task<int> encode(Objects.encoding_config encoding_config, string pick_up_after)
        {
            ffmpeg_original_files.Remove(encoding_config.output_file);
            ffmpeg_original_files.Add(encoding_config.output_file, encoding_config.input_file);

            string arguments = await get_arguments(encoding_config, pick_up_after);

            if (pick_up_after != null)
            {
                arguments = "-y " + arguments;
            }

            Process ffmpeg_cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)), "ffmpeg.exe"),
                    Arguments = arguments + " -flush_packets 1",
                    // UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            ffmpeg_cmd.Start();

            ffmpeg_processes.Remove(encoding_config.output_file);
            ffmpeg_processes.Add(encoding_config.output_file, ffmpeg_cmd);

            // TODO: when the timing is bad during pausing, this sometimes fires "StandardError has not been redirected".
            string process_output = null;
            while ((process_output = ffmpeg_cmd.StandardError.ReadLine()) != null)
            {
                try {
                    Console.WriteLine("FFMPEG process_output "+process_output);
                    ffmpeg_progress.Remove(encoding_config.output_file);
                    ffmpeg_progress.Add(encoding_config.output_file, get_time_encoded(process_output));
                } catch (Exception e) {
                    Console.WriteLine("FFMPEG PROGRESS ERROR "+e);
                }
            }

            ffmpeg_cmd.WaitForExit();
            Console.WriteLine("FFMPEG process has exited.");
            
            ffmpeg_processes.Remove(encoding_config.output_file);

            int statusCode = ffmpeg_cmd.ExitCode;

            Debug.WriteLine($"FFMPEG Exit Code: {statusCode}");

            return statusCode;
        }
        public static string get_time_encoded(string ffmpeg_output)
        {
            try {
            // Parse the FFmpeg output to find the duration and time encoded
                if (ffmpeg_output.Contains("time="))
                {
                    // Extract the current time encoded
                    string time_encoded = ffmpeg_output.Substring(ffmpeg_output.IndexOf("time=") + 5, 11);
                    Debug.WriteLine($"Time Encoded: {time_encoded}");

                    return time_encoded;
                } else {
                    return null;
                }
            } catch (Exception e) {
                Console.WriteLine("get_time_encoded ERROR "+e);
                return null;
            }
        }
        public static async void play(string path)
        {
            try {
                // TODO: This will break if path changes from being /processing to the actual original file.
                string pick_up_after = await SecureStorage.Default.GetAsync("pick_up_after_"+path);
                if (pick_up_after == null)
                {
                    // TODO: handle no value here.
                    Debug.WriteLine("FFMPEG NOT VALUE FOR PICK_UP_AFTER");
                    return;
                }

                string original_file = Files.is_valid_path(ffmpeg.ffmpeg_original_files[path]);
                Objects.ffprobevideoinfo ffprobe = ffmpeg.ffprobe(original_file);
                Objects.encoding_config encoding_config_from_ffprobe = ffmpeg.get_encoding_config_from_ffprobe(ffprobe, path);
                int exit_code = await ffmpeg.encode(encoding_config_from_ffprobe, pick_up_after);

                SecureStorage.Remove("pick_up_after_"+path);
                if (exit_code == 0) {
                    ffmpeg.successful_clean_up(encoding_config_from_ffprobe.input_file, encoding_config_from_ffprobe.output_file);
                }
            } catch (Exception e) {
                Console.WriteLine("ffmpeg play ERROR "+e);
            }
        }
        public static async Task<bool> pause(string path, Objects.queue_item queue_item)
        {
            Debug.WriteLine("STOPPING APPLICATION: PAUSE: "+path);
            // TODO: This will break if path changes from being /processing to the actual original file.
            ffmpeg.ffmpeg_processes[path].Kill();
            ffmpeg.ffmpeg_processes.Remove(path);
            await SecureStorage.SetAsync("pick_up_after_"+path, queue_item.encoded);

            return true;
        }
        public static async void successful_clean_up(string input_file, string output_file)
        {
            queue.queue_path.Remove(output_file);

            FileInfo fileInfo = new FileInfo(input_file);
            long file_size_in_bytes = fileInfo.Length;

            string file_name = Path.GetFileName(input_file);
            Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
            
            string processed_input_file_directory = relevant_directories.input_directory_processed;
            Files.move_file_with_duplicate_protection(input_file, processed_input_file_directory+@"\"+file_name);

            Files.move_file_with_duplicate_protection(output_file, relevant_directories.output_directory+@"\"+file_name);

            await Stats.add_saved(Stats.convert_bytes_to_GB(file_size_in_bytes));
        }

        public static async Task<List<string>> get_formats()
        {
            try {
                Process formats_cmd = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)), "ffprobe.exe"),
                        Arguments = $"-formats",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true,
                };

                formats_cmd.Start();

                string output = formats_cmd.StandardOutput.ReadToEnd();
                string error = formats_cmd.StandardError.ReadToEnd();

                formats_cmd.WaitForExit();

                int status_code = formats_cmd.ExitCode;

                if (status_code != 0) {
                    Debug.WriteLine("Formats ffmpeg exit status code was not 0");
                    return new List<String>(); // TODO: Throw error;
                }

                List<string> formats = new List<String>();

                string[] lines = output.Split('\n');

                bool has_started = false;

                foreach (string line in lines)
                {
                    if (line.Contains("---")) {
                        has_started = true;
                    } else if (has_started == true && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        bool can_decode = parts[0].Contains("D");
                        bool can_encode = parts[0].Contains("E");

                        List <string> parts_new = parts.ToList<string>();

                        string code = parts_new[1];
                        string description = line.Substring(line.IndexOf(code) + code.Length).Trim();

                        if (can_encode == true)
                        {
                            formats.Add(code);
                        }
                    }
                }

                Debug.WriteLine("Formats finished fine");

                return formats;
            } catch (Exception e)
            {
                Debug.WriteLine("FORMATS ERROR "+e);
                return new List<String>();
            }
        }

        public static async Task<List<string>> get_codecs()
        {
            Process codecs_cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ffprobe.exe"),
                    Arguments = $"-codecs",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
            };

            codecs_cmd.Start();

            string output = codecs_cmd.StandardOutput.ReadToEnd();
            string error = codecs_cmd.StandardError.ReadToEnd();

            codecs_cmd.WaitForExit();

            int status_code = codecs_cmd.ExitCode;

            if (status_code != 0) {
                Debug.WriteLine("Codecs ffmpeg exit status code was not 0");
                return new List<String>(); // TODO: Throw error;
            }

            List<string> codecs = new List<String>();

            string[] lines = output.Split('\n');

            bool has_started = false;

            foreach (string line in lines)
            {
                if (line.Contains("---")) {
                    has_started = true;
                } else if (has_started == true && !string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    bool is_video = parts[0].Contains("V");

                    List <string> parts_new = parts.ToList<string>();

                    string code = parts_new[1];
                    string description = line.Substring(line.IndexOf(code) + code.Length).Trim();

                    if (is_video == true) {
                        codecs.Add(code);
                    }
                }
            }

            return codecs;
        }
    }
}
