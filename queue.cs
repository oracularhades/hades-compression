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
    public class queue
    {
        public static int in_progress = 0;
        public static List<string> queue_path = new List<string>();
        public static List<string> was_compressing_when_paused = new List<string>();
        public static bool was_paused = false;
        // TODO: Check all outputs were successful, and return an error if they weren't.
        public static async Task<List<Objects.queue_item>> get()
        {
            try {
                Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
                string input_directory = relevant_directories.input_directory;
                string output_directory = relevant_directories.output_directory;
                string input_directory_processing = relevant_directories.input_directory_processing;

                Files.get_available_unavailable_files_in_directory_output input_directory_status = Files.get_available_unavailable_files_in_directory(relevant_directories.input_directory);
                Files.get_available_unavailable_files_in_directory_output output_directory_status = Files.get_available_unavailable_files_in_directory(output_directory);
                Files.get_available_unavailable_files_in_directory_output input_directory_processing_status = Files.get_available_unavailable_files_in_directory(input_directory_processing);
                
                List<Objects.queue_item> queue = new List<Objects.queue_item>();

                queue_path.ForEach(element =>
                {
                    string file_name = Path.GetFileName(element);

                    string input_directory_filepath = input_directory + @$"\{file_name}";
                    string input_directory_processing_filepath = input_directory_processing + @$"\{file_name}";

                    if (ffmpeg.ffmpeg_processes.ContainsKey(element)) {
                        try {
                            Debug.WriteLine("COMPRESSING: ");

                            string max_encoded = "";
                            if (ffmpeg.ffprobe_video_length.ContainsKey(input_directory_filepath)) {
                                // max_encoded = max_encoded.Substring(0, Math.Min(ffmpeg.ffprobe_video_length[input_directory_filepath].Length, 11));
                                max_encoded = ffmpeg.ffprobe_video_length[input_directory_filepath];
                            }

                            string encoded = "";
                            if (ffmpeg.ffmpeg_progress.ContainsKey(element)) {
                                encoded = ffmpeg.ffmpeg_progress[element];
                            }
                            
                            queue.Add(new Objects.queue_item {
                                status = "Compressing",
                                path = element,
                                paused = false,
                                compressing = true,
                                encoded = encoded,
                                max_encoded = max_encoded
                            });
                        } catch (Exception e) {
                            Console.WriteLine("\"compressing\" queue error: "+e);
                        }
                    } else {
                        Random random = new Random();
                        queue.Add(new Objects.queue_item {
                            status = "Paused",
                            path = element,
                            paused = true,
                            compressing = false,
                            encoded = null,
                            max_encoded = null
                        });
                    }
                });

                // new Objects.queue_item {
                //     status = "Waiting",
                //     path = path,
                //     paused = true,
                //     compressing = false,
                //     encoded = null,
                //     max_encoded = null
                // }

                return queue;
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return new List<Objects.queue_item>();
            }
        }
        public static async Task<List<Objects.queue_item>> get_compressing()
        {
            List<Objects.queue_item> queue_items = await queue.get();

            List<Objects.queue_item> compressing = new List<Objects.queue_item>();
            foreach (Objects.queue_item item in queue_items)
            {
                if (item.compressing != true) { continue; }
                compressing.Add(item);
            }

            return compressing;
        }
        public static async Task<int> queue_ffmpeg(string path)
        {
            while (true)
            {
                Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
                string output_directory = Files.is_valid_path(relevant_directories.input_directory_processing+@"\"+Path.GetFileName(path));

                if (queue_path.IndexOf(output_directory) != 0)
                {
                    // We're not front of the queue yet.
                    Debug.WriteLine("queue_ffmpeg Not front of queue yet.");
                } else
                {
                    Objects.settings settings = await Settings.get();
                    if (in_progress >= settings.thread_limit)
                    {
                        // Over concurrent encoding limit.
                        Debug.WriteLine("queue_ffmpeg Tried to start, but over concurrent encoding limit.");
                    } else {
                        Debug.WriteLine("queue_ffmpeg Starting... "+path);
                        
                        Objects.ffprobevideoinfo ffprobe = ffmpeg.ffprobe(path);

                        Objects.encoding_config encoding_config_from_ffprobe = ffmpeg.get_encoding_config_from_ffprobe(ffprobe, output_directory);

                        in_progress = in_progress+1;
                        int exit_code = await ffmpeg.encode(encoding_config_from_ffprobe, null);

                        Debug.WriteLine($"queue_ffmpeg FFMPEG EXIT CODE: {exit_code}");
                        if (exit_code == 0) {
                            ffmpeg.successful_clean_up(encoding_config_from_ffprobe.input_file, encoding_config_from_ffprobe.output_file);
                        }

                        in_progress = in_progress-1;

                        return exit_code;
                    }
                }

                Thread.Sleep(1000);
            }
        }
        public static async Task<bool> pause_all()
        {
            List<Objects.queue_item> queue_items = await queue.get();
            foreach (Objects.queue_item item in queue_items)
            {
                if (item.compressing != true) { continue; }
                await ffmpeg.pause(item.path, item);
                was_compressing_when_paused.Remove(item.path);
                was_compressing_when_paused.Add(item.path);
            }

            was_paused = true;

            return true;
        }
        public static async Task<bool> resume_all()
        {
            foreach (string path in was_compressing_when_paused)
            {
                ffmpeg.play(path);
            }

            was_paused = false;

            return true;
        }
    }
}
