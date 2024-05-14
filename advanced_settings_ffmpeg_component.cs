using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HadesCompression;

public class AdvancedSettingsFFmpegStruct : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _output_format;
    public string output_format
    {
        get => _output_format;
        set
        {
            if (_output_format != value)
            {
                _output_format = value;
                OnPropertyChanged();
            }
        }
    }

    private string _preset;
    public string preset
    {
        get => _preset;
        set
        {
            if (_preset != value)
            {
                _preset = value;
                OnPropertyChanged();
            }
        }
    }

    private string _max_quality;
    public string max_quality
    {
        get => _max_quality;
        set
        {
            if (_max_quality != value)
            {
                _max_quality = value;
                OnPropertyChanged();
            }
        }
    }

    private string _codec;
    public string codec
    {
        get => _codec;
        set
        {
            if (_codec != value)
            {
                _codec = value;
                OnPropertyChanged();
            }
        }
    }

    private int _max_video_bitrate;
    public int max_video_bitrate
    {
        get => _max_video_bitrate;
        set
        {
            if (_max_video_bitrate != value)
            {
                _max_video_bitrate = value;
                OnPropertyChanged();
            }
        }
    }

    private int _max_audio_bitrate;
    public int max_audio_bitrate
    {
        get => _max_audio_bitrate;
        set
        {
            if (_max_audio_bitrate != value)
            {
                _max_audio_bitrate = value;
                OnPropertyChanged();
            }
        }
    }

    private int _crf;
    public int crf
    {
        get => _crf;
        set
        {
            if (_crf != value)
            {
                _crf = value;
                OnPropertyChanged();
            }
        }
    }

    private ObservableCollection<string> _formats = new ObservableCollection<string> {"Loading"};
    public ObservableCollection<string> Formats
    {
        get => _formats;
        set
        {
            if (_formats != value)
            {
                _formats = value;
                OnPropertyChanged();
            }
        }
    }

    private ObservableCollection<string> _codecs = new ObservableCollection<string> {"Loading"};
    public ObservableCollection<string> Codecs
    {
        get => _codecs;
        set
        {
            if (_codecs != value)
            {
                _codecs = value;
                OnPropertyChanged();
            }
        }
    }
}

public partial class AdvancedSettingsFFmpegComponent : ContentView, INotifyPropertyChanged
{
    public bool update_safety_lock = true;

    public AdvancedSettingsFFmpegStruct Advanced_settings_ffmpeg_component = new AdvancedSettingsFFmpegStruct{};

	private async void advanced_settings_update()
    {
        if (update_safety_lock == true)
        {
            return;
        }
        Objects.advanced_settings settings = await advanced.get();
        settings.output_format = Advanced_settings_ffmpeg_component.output_format;
        settings.preset = Advanced_settings_ffmpeg_component.preset;
        settings.max_quality = Advanced_settings_ffmpeg_component.max_quality;
        settings.codec = Advanced_settings_ffmpeg_component.codec;
        settings.max_video_bitrate = Advanced_settings_ffmpeg_component.max_video_bitrate;
        settings.max_audio_bitrate = Advanced_settings_ffmpeg_component.max_audio_bitrate;
        settings.crf = Advanced_settings_ffmpeg_component.crf;

        this.Dispatcher.Dispatch(async () => {
            advanced.update(settings);
        });
	}
    
	public AdvancedSettingsFFmpegComponent()
	{
		InitializeComponent();
		BindingContext = Advanced_settings_ffmpeg_component;

        Advanced_settings_ffmpeg_component.PropertyChanged += (sender, e) =>
        {
            Console.WriteLine($"{e.PropertyName} property has changed.");
            advanced_settings_update();
        };

		Task.Run(async () => {
			List<string> formats_v = await ffmpeg.get_formats();
			ObservableCollection<string> formats_as_collection = new ObservableCollection<string>();
            formats_as_collection.Add("Same as video");
			foreach (var item in formats_v)
			{
				formats_as_collection.Add(item);
			}

			List<string> codecs_v = await ffmpeg.get_codecs();
			ObservableCollection<string> codecs_as_collection = new ObservableCollection<string>();
            codecs_as_collection.Add("Same as video");
			foreach (var item in codecs_v)
			{
				codecs_as_collection.Add(item);
			}

			Objects.advanced_settings advanced_settings = await advanced.get();

            AdvancedSettingsFFmpegStruct Advanced_custom_command_component_v = Advanced_settings_ffmpeg_component;
            Advanced_custom_command_component_v.Formats = formats_as_collection;
            Advanced_custom_command_component_v.Codecs = codecs_as_collection;

            Advanced_custom_command_component_v.output_format = advanced_settings.output_format;
            Advanced_custom_command_component_v.preset = advanced_settings.preset;
            Advanced_custom_command_component_v.max_quality = advanced_settings.max_quality;
            Advanced_custom_command_component_v.codec = advanced_settings.codec;
            Advanced_custom_command_component_v.max_video_bitrate = advanced_settings.max_video_bitrate;
            Advanced_custom_command_component_v.max_audio_bitrate = advanced_settings.max_audio_bitrate;
            Advanced_custom_command_component_v.crf = advanced_settings.crf;
            Advanced_settings_ffmpeg_component = Advanced_custom_command_component_v;

            update_safety_lock = false;
		});
	}
}