using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HadesCompression
{
    public class Files
    {
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
                if (file.EndsWith(".mkv") == false && file.EndsWith(".mp4") == false)
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
        
        public static Objects.get_hadescompression_directories get_hadescompression_directories()
        {
            return new Objects.get_hadescompression_directories {
                input_directory = @"C:\Users\User\Videos",
                output_directory = @"C:\Users\User\output_dir",
                input_directory_processing = @"C:\Users\User\Videos\processing"
            };
        }
    }
}