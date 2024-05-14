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
using System.Text.RegularExpressions;

namespace HadesCompression
{
    public class advanced
    {
        public static async Task<Objects.advanced_settings> get()
        {
            Objects.advanced_settings advanced_settings_object = new Objects.advanced_settings {};

            string output_format_value = await SecureStorage.Default.GetAsync("advanced_settings");
            if (output_format_value != null)
            {
                Objects.advanced_settings deserialized_output = JsonConvert.DeserializeObject<Objects.advanced_settings>(output_format_value);
                if (deserialized_output != null)
                {
                    advanced_settings_object = deserialized_output;
                }
            }

            try {
                return advanced_settings_object;
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return null;
            }
        }
        public static async void update(Objects.advanced_settings advanced_settings) {
            Objects.advanced_settings existing = await get();

            if (object.Equals(existing, advanced_settings)) {
                throw new Exception("Advanced settings doesn't need updating because it's already the same.");
            }

            Regex regex = new Regex("[^A-Za-z0-9:.]");
            if (regex.IsMatch(advanced_settings.obs_address) != false)
            {
                // OBS address contains illegal characters.
                throw new Exception("OBS address contains illegal characters.");
            }

            string output = JsonConvert.SerializeObject(advanced_settings);
            await SecureStorage.Default.SetAsync("advanced_settings", output);
        }
    }
}
