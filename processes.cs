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
    public class processes
    {
        public static bool check_if_another_instance_of_hades_compression()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("HadesCompression");
                if (processes.Length > 1) {
                    Debug.WriteLine("INTENTIONALLY CRASHING BECAUSE ANOTHER INSTANCE OF HADESCOMPRESSION IS OPEN. THIS IS NOT A BUG, THIS IS INTENTIONAL. DONT GET CONFUSED.");
                    return true;
                } else {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking process: {ex.Message}");

                return true;
            }
        }
    }
}