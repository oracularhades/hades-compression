using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HadesCompression;

public class AdvancedPageBindingsStruct : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

	private string _App_version = "";
    public string App_version
    {
        get => _App_version;
        set
        {
            if (_App_version != value)
            {
                _App_version = value;
                OnPropertyChanged();
            }
        }
    }

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
                OnPropertyChanged();
            }
        }
    }
}

public partial class Advanced_Page : ContentPage, INotifyPropertyChanged
{
	public AdvancedPageBindingsStruct AdvancedPage_Bindings = new AdvancedPageBindingsStruct{};

	public async void ffmpeg_command_type_logic()
	{
		Objects.advanced_settings settings = await advanced.get();

		if (AdvancedPage_Bindings.Ffmpeg_command_type == "Settings" || AdvancedPage_Bindings.Ffmpeg_command_type == "settings")
		{
			advanced_custom_command_component.IsVisible = false;
			advanced_settings_component.IsVisible = true;

			if (AdvancedPage_Bindings.Ffmpeg_command_type == "Settings") {
				settings.ffmpeg_command_type = "settings";
				this.Dispatcher.Dispatch(async () => {
					advanced.update(settings);
				});
			}

			AdvancedPage_Bindings.Ffmpeg_command_type = "Settings";
		} else if (AdvancedPage_Bindings.Ffmpeg_command_type == "Custom FFMPEG command" || AdvancedPage_Bindings.Ffmpeg_command_type == "custom_ffmpeg_command")
		{
			advanced_custom_command_component.IsVisible = true;
			advanced_settings_component.IsVisible = false;

			if (AdvancedPage_Bindings.Ffmpeg_command_type == "Custom FFMPEG command")
			{
				settings.ffmpeg_command_type = "custom_ffmpeg_command";
				this.Dispatcher.Dispatch(async () => {
					advanced.update(settings);
				});
			}

			AdvancedPage_Bindings.Ffmpeg_command_type = "Custom FFMPEG command";
		}
	}
	
	public Advanced_Page()
	{
		InitializeComponent();
		BindingContext = AdvancedPage_Bindings;
		
		AdvancedPage_Bindings.App_version = "Version "+VersionTracking.CurrentVersion;
		advanced_custom_command_component.IsVisible = false;
		advanced_settings_component.IsVisible = false;

		Task.Run(async () => {
            Objects.advanced_settings advanced_settings = await advanced.get();

			AdvancedPageBindingsStruct AdvancedPage_Bindings_v = AdvancedPage_Bindings;
			AdvancedPage_Bindings_v.Ffmpeg_command_type = advanced_settings.ffmpeg_command_type;
			AdvancedPage_Bindings = AdvancedPage_Bindings_v;

			AdvancedPage_Bindings.PropertyChanged += (sender, e) => {
				ffmpeg_command_type_logic();
			};
		});
	}
}