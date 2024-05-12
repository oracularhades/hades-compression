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
        public static async void start_watching()
        {
            while (true)
            {
                Debug.WriteLine("STARTED WATCHING");
                Thread.Sleep(1000);

                Objects.settings settings = await Settings.get();

                Debug.WriteLine("STARTED WATCHING past settings");

                bool block_because_of_status = await Obs.block_because_of_status();
                if (block_because_of_status == true) {
                    // User doesn't want to encode because OBS is doing something (streaming/recording/etc), so we don't.
                    Debug.WriteLine("Autocompress.Filehandling.start_watching() is block_because_of_status");
                } else {
                    if (settings.automatically_compress_videos == false) {
                        Debug.WriteLine("get_automatically_compress_videos is false");
                        return;
                    }

                    Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
                    string input_directory = relevant_directories.input_directory;
                    string output_directory = relevant_directories.output_directory;
                    string input_directory_processing = relevant_directories.input_directory_processing;

                    Files.get_available_unavailable_files_in_directory_output input_directory_status = Files.get_available_unavailable_files_in_directory(input_directory);
                    Files.get_available_unavailable_files_in_directory_output output_directory_status = Files.get_available_unavailable_files_in_directory(output_directory);
                    Files.get_available_unavailable_files_in_directory_output input_directory_processing_status = Files.get_available_unavailable_files_in_directory(input_directory_processing);

                    Debug.WriteLine("ELEMENT 111");
                    input_directory_status.available_files.ForEach(element =>
                    {
                        Debug.WriteLine("ELEMENT "+element);
                        string file_name = Path.GetFileName(element);

                        string input_directory_processing_filepath = input_directory_processing + @$"\{file_name}";

                        if (queue.queue_path.Contains(input_directory_processing_filepath))
                        {
                        } else {
                            Debug.WriteLine("QUEUING FFMPEG "+input_directory_processing_filepath);

                            queue.queue_path.Add(input_directory_processing_filepath);

                            Task.Run(async () => queue.queue_ffmpeg(element));
                        }
                    });
                }
            }
        }
    }
}
