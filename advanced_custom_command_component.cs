using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class AdvancedCustomCommandComponent : ContentView, INotifyPropertyChanged
{
    public bool update_safety_lock = true;
	private string _ffmpeg_command = null;
    public string Ffmpeg_command
    {
        get => _ffmpeg_command;
        set
        {
            if (_ffmpeg_command != value)
            {
                _ffmpeg_command = value;
                AdvancedSettingsChange();
                OnPropertyChanged();
            }
        }
    }

    private string _ffmpeg_variables = null;
    public string Ffmpeg_variables
    {
        get => _ffmpeg_variables;
        set
        {
            if (_ffmpeg_variables != value)
            {
                _ffmpeg_variables = value;
                OnPropertyChanged();
            }
        }
    }

    private async void AdvancedSettingsChange()
    {
        if (update_safety_lock == true)
        {
            return;
        }
        
        Objects.advanced_settings settings = await advanced.get();
        settings.ffmpeg_custom_command = Ffmpeg_command;

        this.Dispatcher.Dispatch(async () => {
            advanced.update(settings);
        });
	}

	public AdvancedCustomCommandComponent()
	{
		InitializeComponent();
		BindingContext = this;

		Task.Run(async () => {
            Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
			string arguments = await ffmpeg.get_arguments(new Objects.encoding_config {
                input_file = relevant_directories.input_directory,
                output_file = relevant_directories.output_directory,
                audio_tracks = new List<int> { 0 }
            }, null);

            Objects.advanced_settings advanced_settings = await advanced.get();

            string ffmpeg_output_command = null;
            if (advanced_settings.ffmpeg_custom_command != null) {
                ffmpeg_output_command = advanced_settings.ffmpeg_custom_command;
            } else {
                string clothed_ffmpeg_command = "ffmpeg "+arguments;
                clothed_ffmpeg_command = clothed_ffmpeg_command.Replace($"\"{relevant_directories.input_directory}\"", "[input_directory]");
                clothed_ffmpeg_command = clothed_ffmpeg_command.Replace($"\"{relevant_directories.output_directory}\"", "[output_directory]");
                clothed_ffmpeg_command = clothed_ffmpeg_command.Replace("-map 0:a:0", "[audio_tracks]");

                ffmpeg_output_command = clothed_ffmpeg_command;
            }

            Ffmpeg_command = ffmpeg_output_command;
            Ffmpeg_variables = File.ReadAllText("ffmpeg_variable_description.txt");
            update_safety_lock = false;
		});
	}
}