using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.Reflection;

namespace HadesCompression
{
    public class Settings
    {
        public static async Task<Objects.settings> get()
        {
            Objects.settings settings_object = new Objects.settings {};

            string output_format_value = await SecureStorage.Default.GetAsync("settings");
            if (output_format_value != null)
            {
                Debug.WriteLine("Settingsget "+output_format_value);
                Objects.settings deserialized_output = JsonConvert.DeserializeObject<Objects.settings>(output_format_value);
                if (deserialized_output != null)
                {
                    settings_object = deserialized_output;
                }
            }

            int cpu_limit = settings_object.cpu_limit;
            int memory_limit = settings_object.memory_limit;
            int thread_limit = settings_object.thread_limit;
            bool encode_while_recording = settings_object.encode_while_recording;
            bool encode_while_streaming = settings_object.encode_while_streaming;

            try {
                return new Objects.settings {
                    cpu_limit = cpu_limit,
                    memory_limit = memory_limit,
                    thread_limit = thread_limit,
                    encode_while_recording = encode_while_recording,
                    encode_while_streaming = encode_while_streaming
                };
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return null;
            }
        }
        public static async void update(Objects.settings settings) {
            Objects.settings existing = await get();

            Debug.WriteLine("SETTINGS EXISTING "+JsonConvert.SerializeObject(existing));
            Debug.WriteLine("SETTINGS settings "+JsonConvert.SerializeObject(settings));

            if (object.Equals(existing, settings)) {
                Debug.WriteLine("Settings doesn't need updating because it's already the same.");
                return;
            }

            string output = JsonConvert.SerializeObject(settings);
            await SecureStorage.Default.SetAsync("settings", output);
        }
    }
}
