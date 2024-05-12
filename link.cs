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
    public class link
    {
        public static void open(string link)
        {
            if (link.StartsWith("https://") == false)
            {
                throw new Exception("Link must start with https://");
            }
            if (is_valid_link(link) != true)
            {
                // Stop any command injection nonsense.
                throw new Exception("Invalid link.");
            }

            // Create a ProcessStartInfo object with the URL as the filename
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            // Open the URL in the default web browser
            Process.Start(psi);
        }
        public static bool is_valid_link(string link)
        {
            try
            {
                Uri uri = new Uri(link);

                if (uri.IsWellFormedOriginalString())
                {
                    Console.WriteLine("URL is well-formed.");
                    return true;
                }
                else
                {
                    Console.WriteLine("URL is not well-formed.");
                    return false;
                }
            }
            catch (UriFormatException)
            {
                Console.WriteLine("URL is not well-formed.");
                return false;
            }
        }
    }
}