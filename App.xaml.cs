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
            InitializeComponent();

            MainPage = new AppShell();

            if (processes.check_if_another_instance_of_hades_compression() == true)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);

            window.Destroying += async (s, e) =>
            {
                Debug.WriteLine("WINDOW DESTROYING");

                if (processes.check_if_another_instance_of_hades_compression() == false)
                {
                    await queue.pause_all();
                }
            };

            window.Width = 800;

            return window;
        }
    }
}