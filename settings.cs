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
        public static int max_cpu_resource_limit = 70;
        public static int max_memory_resource_limit = 70;
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
            Debug.WriteLine("SETTINGS UPDATE RUNNING");
            Objects.settings existing = await get();

            if (object.Equals(existing, settings)) {
                throw new Exception("Settings doesn't need updating because it's already the same.");
            }

            if (check_resource_threads_under_limit(settings.cpu_limit, settings.thread_limit) == false) {
                settings.cpu_limit = max_cpu_resource_limit;
                settings.thread_limit = 1;
            }
            if (check_resource_threads_under_limit(settings.memory_limit, settings.thread_limit) == false) {
                settings.memory_limit = max_memory_resource_limit;
                settings.thread_limit = 1;
            }

            string preflight_check = preflight(settings);
            if (preflight_check != null)
            {
                throw new Exception(preflight_check);
            }

            string output = JsonConvert.SerializeObject(settings);
            await SecureStorage.Default.SetAsync("settings", output);

            if (existing.thread_limit != settings.thread_limit || existing.cpu_limit != settings.cpu_limit || existing.memory_limit != settings.memory_limit)
            {
                await queue.pause_all();
                await queue.resume_all();
            }
        }
        public static bool check_resource_threads_under_limit(int value, int threads)
        {
            if (threads * value > 90)
            {
                return false;
            }

            return true;
        }
        public static string preflight(Objects.settings settings)
        {
            if (check_resource_threads_under_limit(settings.cpu_limit, settings.thread_limit) == false) {
                return "Cpu limit over threads * value limit";
            }
            if (check_resource_threads_under_limit(settings.memory_limit, settings.thread_limit) == false) {
                return "Memory limit over threads * value limit";
            }

            if (settings.input_directory == settings.output_directory)
            {
                return "input_directory cannot be output_directory";
            }

            return null;
        }
    }
}
