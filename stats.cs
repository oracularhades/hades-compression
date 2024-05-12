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
    public class Stats
    {
        public static async Task<Objects.stats> get()
        {
            Objects.stats stats_object = new Objects.stats {};

            string output_format_value = await SecureStorage.Default.GetAsync("stats");
            if (output_format_value != null)
            {
                Objects.stats deserialized_output = JsonConvert.DeserializeObject<Objects.stats>(output_format_value);
                if (deserialized_output != null)
                {
                    stats_object = deserialized_output;
                }
            }

            long storage_saved_GB = stats_object.storage_saved_GB;

            try {
                return new Objects.stats {
                    storage_saved_GB = storage_saved_GB
                };
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return null;
            }
        }
        public static async void update(Objects.stats stats) {
            Objects.stats existing = await get();

            if (object.Equals(existing, stats)) {
                return;
            }

            string output = JsonConvert.SerializeObject(stats);
            await SecureStorage.Default.SetAsync("stats", output);
        }
        public static async Task<long> add_saved(long saved_in_GB) {
            Objects.stats existing = await get();
            
            long new_stat = existing.storage_saved_GB + saved_in_GB;
            existing.storage_saved_GB = new_stat;

            Stats.update(existing);

            return new_stat;
        }
        public static long convert_bytes_to_GB(long bytes)
        {
            // # AI generated
            const long bytesInGB = 1024 * 1024 * 1024;
            return (long)bytes / bytesInGB;
        }
    }
}