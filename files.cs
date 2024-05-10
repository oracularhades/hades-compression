using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;

namespace HadesCompression
{
    public class Files
    {
        public static List<String> allowed_file_types = new List<string> {"mkv", "mp4", "avi"};
        
        public class get_available_unavailable_files_in_directory_output
        {
            public List<String> all { get; set; }
            public List<String> available_files { get; set; }
            public List<String> unavailable_files { get; set; }
        }

        public static get_available_unavailable_files_in_directory_output get_available_unavailable_files_in_directory(string directory_path)
        {
            IEnumerable<string> files = Directory.GetFiles(directory_path);

            List<String> all = new List<String>();
            List<String> available_files = new List<String>();
            List<String> unavailable_files = new List<String>();

            foreach (var file in files)
            {
                if (allowed_file_types.Contains(file) == false)
                {
                    // This file isn't what we're looking for.
                    continue;
                }

                try
                {
                    using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        // File is not locked
                        // Debug.WriteLine($"File {file} is not locked and is accessible.");
                        available_files.Add(file);
                        all.Add(file);
                    }
                }
                catch (IOException error)
                {
                    Debug.WriteLine($"File {file} is locked or in-use.");
                    unavailable_files.Add(file);
                    all.Add(file);
                }
            }

            return new get_available_unavailable_files_in_directory_output
            {
                available_files = available_files,
                unavailable_files = unavailable_files,
                all = all
            };
        }

        public static string is_valid_path(string file_path)
        {
            // this function will throw an error if the filepath is invalid.
            Path.GetFullPath(file_path);

            return file_path;
        }
        
        public static async Task<Objects.get_hadescompression_directories> get_hadescompression_directories()
        {
            Objects.settings settings = await Settings.get();

            string input_directory = settings.input_directory;
            string output_directory = settings.output_directory;

            if (input_directory == null)
            {
                input_directory = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Videos";
            }

            if (output_directory == null)
            {
                output_directory = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\output_dir";
            }

            string processing_directory = input_directory+@"\processing";
            
            Directory.CreateDirectory(input_directory);
            Directory.CreateDirectory(output_directory);
            Directory.CreateDirectory(processing_directory);

            return new Objects.get_hadescompression_directories {
                input_directory = input_directory,
                output_directory = output_directory,
                input_directory_processing = processing_directory
            };
        }
        public static string formatted_output(double size_in_GB)
        {
            // # AI generated
            if (size_in_GB >= 1024) // If size is in terabytes or more
            {
                double terabytes = size_in_GB / 1024;
                if (terabytes >= 1024) // If size is in petabytes or more
                {
                    double petabytes = terabytes / 1024;
                    return $"{petabytes:F2}PB";
                }
                else
                {
                    return $"{terabytes:F2}TB";
                }
            }
            else
            {
                return $"{size_in_GB:F2}GB";
            }
        }
        public static void download_file(string url, string destination)
        {
            using (WebClient webClient = new WebClient())
            {
                // Ensure the destination folder exists
                Directory.CreateDirectory(destination);

                // Download the file
                webClient.DownloadFile(url, destination);
            }
        }
        public static void unzip_file(string zipFilePath, string destinationFolder)
        {
            ZipFile.ExtractToDirectory(zipFilePath, destinationFolder);
        }
        public static void add_to_path(string folder_path)
        {
            Files.is_valid_path(folder_path);

            string current_path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

            // Check if the folder is already in the PATH
            if (!current_path.Contains(folder_path))
            {
                // Append the folder path to the PATH variable
                string new_path = current_path + ";" + folder_path;

                // Update the PATH environment variable
                Environment.SetEnvironmentVariable("PATH", new_path, EnvironmentVariableTarget.Machine);
            }
        }
    }
}