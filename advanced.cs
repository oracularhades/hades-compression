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

            string obs_address = advanced_settings_object.obs_address;
            string obs_password = advanced_settings_object.obs_password;
            string ffmpeg_command_type = advanced_settings_object.ffmpeg_command_type;
            string output_format = advanced_settings_object.output_format;
            string preset = advanced_settings_object.preset;
            string max_quality = advanced_settings_object.max_quality;
            string codec = advanced_settings_object.codec;
            int max_video_bitrate = advanced_settings_object.max_video_bitrate;
            int max_audio_bitrate = advanced_settings_object.max_video_bitrate;
            int crf = advanced_settings_object.crf;

            try {
                return new Objects.advanced_settings {
                    obs_address = obs_address,
                    obs_password = obs_password,
                    ffmpeg_command_type = ffmpeg_command_type,
                    output_format = output_format,
                    preset = preset,
                    max_quality = max_quality,
                    codec = codec,
                    max_video_bitrate = max_video_bitrate,
                    max_audio_bitrate = max_audio_bitrate,
                    crf = crf
                };
            } catch (Exception e) {
                Console.WriteLine("Queue error: "+e);
                return null;
            }
        }
        public static async void update(Objects.advanced_settings advanced_settings) {
            Objects.advanced_settings existing = await get();

            if (object.Equals(existing, advanced_settings)) {
                Debug.WriteLine("Advanced settings doesn't need updating because it's already the same.");
                return;
            }

            Regex regex = new Regex("[^A-Za-z0-9:.]");
            if (regex.IsMatch(advanced_settings.obs_address) != false)
            {
                // OBS address contains illegal characters.
                Debug.WriteLine("OBS address contains illegal characters.");
                return;
            }

            string output = JsonConvert.SerializeObject(advanced_settings);
            await SecureStorage.Default.SetAsync("advanced_settings", output);
        }
    }
}
