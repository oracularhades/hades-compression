using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class Advanced_Page : ContentPage, INotifyPropertyChanged
{
    public bool update_safety_lock = true;
	private string _ffmpeg_command_type = null;
    public string Ffmpeg_command_type
    {
        get => _ffmpeg_command_type;
        set
        {
            if (_ffmpeg_command_type != value)
            {
                _ffmpeg_command_type = value;
				ffmpeg_command_type_logic();
                OnPropertyChanged();
            }
        }
    }

	public async void ffmpeg_command_type_logic()
	{
		Objects.advanced_settings settings = await advanced.get();

		if (Ffmpeg_command_type == "Settings" || Ffmpeg_command_type == "settings")
		{
			advanced_custom_command_component.IsVisible = false;
			advanced_settings_component.IsVisible = true;

			if (Ffmpeg_command_type == "Settings") {
				settings.ffmpeg_command_type = "settings";
				this.Dispatcher.Dispatch(async () => {
					advanced.update(settings);
				});
			}

			Ffmpeg_command_type = "Settings";
		} else if (Ffmpeg_command_type == "Custom FFMPEG command" || Ffmpeg_command_type == "custom_ffmpeg_command")
		{
			advanced_custom_command_component.IsVisible = true;
			advanced_settings_component.IsVisible = false;

			if (Ffmpeg_command_type == "Custom FFMPEG command")
			{
				settings.ffmpeg_command_type = "custom_ffmpeg_command";
				this.Dispatcher.Dispatch(async () => {
					advanced.update(settings);
				});
			}

			Ffmpeg_command_type = "Custom FFMPEG command";
		}
	}
	
	public Advanced_Page()
	{
		InitializeComponent();

		advanced_custom_command_component.IsVisible = false;
		advanced_settings_component.IsVisible = false;

		BindingContext = this;
		Task.Run(async () => {
            Objects.advanced_settings advanced_settings = await advanced.get();

			this.Dispatcher.Dispatch(() => {
				Ffmpeg_command_type = advanced_settings.ffmpeg_command_type;
			});

			ffmpeg_command_type_logic();
		});
	}
}