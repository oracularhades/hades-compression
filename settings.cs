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
                Objects.settings deserialized_output = JsonConvert.DeserializeObject<Objects.settings>(output_format_value);
                if (deserialized_output != null)
                {
                    settings_object = deserialized_output;
                }
            }

            try {
                return settings_object;
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return null;
            }
        }
        public static async void update(Objects.settings settings) {
            Objects.settings existing = await get();

            if (object.Equals(existing, settings)) {
                Debug.WriteLine("Settings doesn't need updating because it's already the same.");
                return;
            }

            string output = JsonConvert.SerializeObject(settings);
            await SecureStorage.Default.SetAsync("settings", output);
        }
    }
}
