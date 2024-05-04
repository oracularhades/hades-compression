using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

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
                    FileName = @"C:\Users\User\Documents\GitHub\HadesCompression\ffmpeg\ffprobe.exe",
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
            int statusCode = ffprobe_cmd.ExitCode;

            // Output the results
            Debug.WriteLine($"Exit Code: {statusCode}");
            Debug.WriteLine($"Output: {output}");
            Debug.WriteLine($"Error: {error}");

            ffprobe_cmd.WaitForExit();

            Objects.ffprobevideoinfo ffprobe_output = JsonConvert.DeserializeObject<Objects.ffprobevideoinfo>(output);

            foreach (var stream in ffprobe_output.streams)
            {
                if (stream.codec_type == "video") {
                    ffprobe_video_length.Remove(file_path);
                    ffprobe_video_length.Add(file_path, stream.tags.DURATION);
                    break;
                }
            }

            return ffprobe_output;
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

        public static async Task<int> encode(Objects.encoding_config encoding_config, string pick_up_after)
        {
            ffmpeg_original_files.Remove(encoding_config.output_file);
            ffmpeg_original_files.Add(encoding_config.output_file, encoding_config.input_file);

            Debug.WriteLine("ffmpeg encode start");
            string arguments = $"-y -i \"{Files.is_valid_path(encoding_config.input_file)}\" -map 0:v -c:a aac";

            string cpu_limit_percent = await SecureStorage.Default.GetAsync("cpu_limit");
            if (cpu_limit_percent != null)
            {
                int cpu_threads = Environment.ProcessorCount;
                double percent_to_threads = cpu_threads * (int.Parse(cpu_limit_percent) / 100.0);
                arguments = arguments + $" --threads {percent_to_threads}";
            }

            string memory_limit_percent = await SecureStorage.Default.GetAsync("memory_limit");
            if (memory_limit_percent != null)
            {
                int memory_bytes = System.Runtime.InteropServices.Marshal.SizeOf(typeof(double));
                double percent_to_bytes = memory_bytes * (int.Parse(memory_limit_percent) / 100.0);
                arguments = arguments + $" --threads {percent_to_bytes}";
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

            Debug.WriteLine("ffmpeg Arguments: "+arguments);

            Process ffmpeg_cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "./ffmpeg/ffmpeg.exe",
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
                Debug.WriteLine("FFMPEG "+process_output);
                try {
                    ffmpeg_progress.Remove(encoding_config.output_file);
                    ffmpeg_progress.Add(encoding_config.output_file, get_time_encoded(process_output));
                    Debug.WriteLine($"AFTER ADDING {ffmpeg_progress[encoding_config.output_file]}");
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
        }
        public static async void pause(string path, Objects.queue_item queue_item)
        {
            Debug.WriteLine("PAUSED OCCURED! "+path);
            // TODO: This will break if path changes from being /processing to the actual original file.
            ffmpeg.ffmpeg_processes[path].Close();
            ffmpeg.ffmpeg_processes.Remove(path);
            await SecureStorage.SetAsync("pick_up_after_"+path, queue_item.encoded);
            Debug.WriteLine("Should have ended process");
        }
        public static async void successful_clean_up(string input_file, string output_file)
        {
            string file_name = Path.GetFileName(input_file);
            Objects.get_hadescompression_directories relevant_directories = Files.get_hadescompression_directories();
            
            string processed_input_file_directory = relevant_directories.input_directory+@"\processed";
            Directory.CreateDirectory(processed_input_file_directory);
            Debug.WriteLine($"MOVE1 {input_file} {processed_input_file_directory+@"\"+file_name}");
            File.Move(input_file, processed_input_file_directory+@"\"+file_name);

            Debug.WriteLine($"MOVE2 {output_file} {relevant_directories.output_directory+@"\"+file_name}");
            File.Move(output_file, relevant_directories.output_directory+@"\"+file_name);
        }
    }
}
