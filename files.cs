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
                bool is_valid_file_extension = false;
                allowed_file_types.ForEach((file_type) => {
                    if (file.EndsWith(file_type)) {
                        is_valid_file_extension = true;
                    }
                });

                if (is_valid_file_extension == false) {
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
            string processed_directory = input_directory+@"\processed";
            
            Directory.CreateDirectory(input_directory);
            Directory.CreateDirectory(output_directory);
            Directory.CreateDirectory(processing_directory);

            return new Objects.get_hadescompression_directories {
                input_directory = input_directory,
                output_directory = output_directory,
                input_directory_processing = processing_directory,
                input_directory_processed = processed_directory
            };
        }
        public static string formatted_output(long size_in_bytes)
        {
            // Convert bytes to gigabytes
            double size_in_GB = size_in_bytes / (1024 * 1024 * 1024);
            
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
        public static void move_file_with_duplicate_protection(string input_file, string output_file)
        {
            Debug.WriteLine("new_full_path input_file "+input_file);
            Debug.WriteLine("new_full_path output_file "+output_file);

            string file_name = Path.GetFileName(output_file);
            string output_directory = Path.GetDirectoryName(output_file);
            int count = 1;

            // Check if the file exists and append a number to create a unique filename
            string file_name_no_extension = Path.GetFileNameWithoutExtension(file_name);
            string extension = Path.GetExtension(output_file);
            string new_full_path = output_file;
            
            Debug.WriteLine("new_full_path "+new_full_path);

            while (File.Exists(new_full_path))
            {
                Debug.WriteLine("new_full_pathEEEEE "+new_full_path);
                string temp_file_name = $"{file_name_no_extension} ({count++})";
                new_full_path = Path.Combine(output_directory, temp_file_name + extension);
            }

            // Move the file to the new path with the unique filename
            File.Move(input_file, new_full_path);
        }
        public static long get_directory_size(string folder_path)
        {
            DirectoryInfo di = new DirectoryInfo(folder_path);
            return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        public static long bytes_to_GB(long bytes)
        {
            return (long)bytes / (1024 * 1024 * 1024);
        }
        public static void delete_files_in_directory(string directory_path)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(directory_path))
                {
                    // Get all file paths within the directory
                    string[] files = Directory.GetFiles(directory_path);

                    // Delete each file
                    foreach (string file in files)
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted file: {file}");
                    }
                }
                else
                {
                    Console.WriteLine("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void open_folder(string directory_path)
        {
            Files.is_valid_path(directory_path); // Make sure this is a valid path, avoid command injection nonsense.

            Process.Start("explorer.exe", directory_path);
        }
    }
}