using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace HadesCompression;

public partial class UpdateAvailableComponent : ContentView, INotifyPropertyChanged
{
    private bool _New_update = false;
    public bool New_update
    {
        get => _New_update;
        set
        {
            if (_New_update != value)
            {
                _New_update = value;
                OnPropertyChanged();
            }
        }
    }

    private string _New_version = "";
    public string New_version
    {
        get => _New_version;
        set
        {
            if (_New_version != value)
            {
                _New_version = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Lastest_download_link = null;
    public string Lastest_download_link
    {
        get => _Lastest_download_link;
        set
        {
            if (_Lastest_download_link != value)
            {
                _Lastest_download_link = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Release_notes = null;
    public string Release_notes
    {
        get => _Release_notes;
        set
        {
            if (_Release_notes != value)
            {
                _Release_notes = value;
                OnPropertyChanged();
            }
        }
    }

    private async void check_update()
    {
        try
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/oracularhades/hades-compression/main/.updater-information");

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the content as a string
                string content = await response.Content.ReadAsStringAsync();
                // Do something with the content
                Console.WriteLine(content);

                Objects.updater_information deserialized_output = JsonConvert.DeserializeObject<Objects.updater_information>(content);

                if (deserialized_output.latest_version != VersionTracking.CurrentVersion)
                {
                    New_update = true;
                    New_version = deserialized_output.latest_version;
                    Lastest_download_link = deserialized_output.latest_download_link;
                    Release_notes = deserialized_output.release_notes;
                }
            }
            else
            {
                Console.WriteLine($"Failed to make request. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    
	private async void OnDownloadUpdate(object sender, EventArgs e)
    {
        link.open(Lastest_download_link);
    }

    private async void OnViewReleaseNotesClicked(object sender, EventArgs e)
    {
        link.open(Release_notes);
    }
    
	public UpdateAvailableComponent()
	{
		InitializeComponent();
		BindingContext = this;

        check_update();
	}
}