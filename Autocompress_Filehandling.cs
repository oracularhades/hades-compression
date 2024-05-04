using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace HadesCompression
{
    public class Autocompress_Filehandling
    {
        public static async Task<bool> get_automatically_compress_videos() {
            string value = await SecureStorage.Default.GetAsync("automatically_compress_videos");
            if (value == null)
            {
                await SecureStorage.Default.SetAsync("automatically_compress_videos", "true");
                return true;
            } else {
                if (value == "true") {
                    return true;
                } else {
                    return false;
                }
            }
        }

        public static async Task<bool> set_automatically_compress_videos(bool value) {
            string output = "";
            if (value == true) {
                output = "true";
            } else {
                output = "false";
            }

            await SecureStorage.Default.SetAsync("automatically_compress_videos", output);

            return value;
        }

        public static async void start_watching()
        {
            while (true)
            {
                Debug.WriteLine("STARTED WATCHING");
                Thread.Sleep(1000);

                Objects.obs_output obs_output = await Obs.get_obs_status(true, true);
                if (obs_output != null && obs_output.stream != null && obs_output.stream.outputActive == true) {
                    // TODO: Check if user wants to not encode whilst streaming.
                } else if (obs_output != null && obs_output.recording != null && obs_output.recording.outputActive == true) {
                } else if (obs_output != null && obs_output.recording != null && obs_output.recording.outputPaused == true) {
                } else {
                    if (await get_automatically_compress_videos() == false) {
                        Debug.WriteLine("get_automatically_compress_videos is false");
                        return;
                    }

                    Objects.get_hadescompression_directories relevant_directories = Files.get_hadescompression_directories();
                    string input_directory = relevant_directories.input_directory;
                    string output_directory = relevant_directories.output_directory;
                    string input_directory_processing = relevant_directories.input_directory_processing;

                    Files.get_available_unavailable_files_in_directory_output input_directory_status = Files.get_available_unavailable_files_in_directory(input_directory);
                    Files.get_available_unavailable_files_in_directory_output output_directory_status = Files.get_available_unavailable_files_in_directory(output_directory);
                    Files.get_available_unavailable_files_in_directory_output input_directory_processing_status = Files.get_available_unavailable_files_in_directory(input_directory_processing);

                    input_directory_status.available_files.ForEach(element =>
                    {
                        string file_name = Path.GetFileName(element);

                        string input_directory_processing_filepath = input_directory_processing + @$"\{file_name}";

                        if (output_directory_status.available_files.Contains(element) || output_directory_status.unavailable_files.Contains(element))
                        {
                            // File already compressed.
                            Debug.WriteLine($"File already compressed: {element}");
                        } else if (input_directory_processing_status.available_files.Contains(input_directory_processing_filepath) || input_directory_processing_status.unavailable_files.Contains(input_directory_processing_filepath))
                        {
                            // File already in queue.
                            Debug.WriteLine($"File already in queue: {element}");
                        } else
                        {
                            // File isn't in queue, we need to add it.

                            // Debug.WriteLine($"TO ADD TO QUEUE: {element}");

                            Directory.CreateDirectory(input_directory+@"\processing");
                            
                            string path = input_directory+@$"\processing\{file_name}";

                            Debug.WriteLine("b4 QUEUING FFMPEG");
                            // Add an empty file to output directory (so this doesn't get flagged again) and place the input file into FFMPEG queue.
                            using (FileStream fs = File.Create(path)) { }

                            Debug.WriteLine("QUEUING FFMPEG");

                            Task.Run(async () => queue.queue_ffmpeg(element));
                        }
                    });
                }
            }
        }
    }
}
