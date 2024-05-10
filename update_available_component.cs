using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class UpdateAvailableComponent : ContentView, INotifyPropertyChanged
{
	private async void OnDownloadUpdate(object sender, EventArgs e)
    {
        string url = "https://example.com";

        // Create a ProcessStartInfo object with the URL as the filename
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };

        // Open the URL in the default web browser
        Process.Start(psi);
    }
    
	public UpdateAvailableComponent()
	{
		InitializeComponent();
		BindingContext = this;
	}
}