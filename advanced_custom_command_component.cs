using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HadesCompression;

public partial class AdvancedCustomCommandComponent : ContentView, INotifyPropertyChanged
{
    public class AdvancedCustomCommandComponentStruct : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _ffmpeg_command;
        public string ffmpeg_command
        {
            get => _ffmpeg_command;
            set
            {
                if (_ffmpeg_command != value)
                {
                    _ffmpeg_command = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ffmpeg_variables;
        public string ffmpeg_variables
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
    }

    private AdvancedCustomCommandComponentStruct Advanced_custom_command_component = new AdvancedCustomCommandComponentStruct{};

    public bool update_safety_lock = true;

	public AdvancedCustomCommandComponent()
	{
		InitializeComponent();
		BindingContext = Advanced_custom_command_component;

        Advanced_custom_command_component.PropertyChanged += (sender, e) =>
        {
            Console.WriteLine($"{e.PropertyName} property has changed.");
        };

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

            AdvancedCustomCommandComponentStruct Advanced_custom_command_component_v = Advanced_custom_command_component;

            Advanced_custom_command_component_v.ffmpeg_command = ffmpeg_output_command;
            Advanced_custom_command_component_v.ffmpeg_variables = File.ReadAllText("ffmpeg_variable_description.txt");

            Advanced_custom_command_component = Advanced_custom_command_component_v;
            update_safety_lock = false;
		});
	}
}