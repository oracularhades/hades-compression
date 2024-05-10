using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Diagnostics;

namespace HadesCompression
{
    public partial class App : Application
    {
        public App()
        {
            // Check for other instances of Hades-Compression.
            try
            {
                Process[] processes = Process.GetProcessesByName("HadesCompression");
                if (processes.Length > 1) {
                    Debug.WriteLine("INTENTIONALLY CRASHING BECAUSE ANOTHER INSTANCE OF HADESCOMPRESSION IS OPEN. THIS IS NOT A BUG, THIS IS INTENTIONAL. DONT GET CONFUSED.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking process: {ex.Message}");
            }

            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
