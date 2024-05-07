using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class AdvancedSettingsComponent : ContentView, INotifyPropertyChanged
{
    public bool update_safety_lock = true;
	private ObservableCollection<string> _formats = new ObservableCollection<string>();
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

	private ObservableCollection<string> _codecs = new ObservableCollection<string>();
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

	private string _output_format = null;
    public string Output_format
    {
        get => _output_format;
        set
        {
            if (_output_format != value)
            {
                _output_format = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private string _preset = null;
    public string Preset
    {
        get => _preset;
        set
        {
            if (_preset != value)
            {
                _preset = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private string _max_quality = null;
    public string Max_quality
    {
        get => _max_quality;
        set
        {
            if (_max_quality != value)
            {
                _max_quality = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private string _codec = null;
    public string Codec
    {
        get => _codec;
        set
        {
            if (_codec != value)
            {
                _codec = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private int _max_video_bitrate = 0;
    public int Max_video_bitrate
    {
        get => _max_video_bitrate;
        set
        {
            if (_max_video_bitrate != value)
            {
                _max_video_bitrate = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private int _max_audio_bitrate = 0;
    public int Max_audio_bitrate
    {
        get => _max_audio_bitrate;
        set
        {
            if (_max_audio_bitrate != value)
            {
                _max_audio_bitrate = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private int _crf = 0;
    public int Crf
    {
        get => _crf;
        set
        {
            if (_crf != value)
            {
                _crf = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private void AdvancedSettingsUpdate()
    {
        Task.Run(async () => {
            if (update_safety_lock == true)
            {
                return;
            }
            Objects.advanced_settings settings = await advanced.get();
            settings.output_format = Output_format;
            settings.preset = Preset;
            settings.max_quality = Max_quality;
            settings.codec = Codec;
            settings.max_video_bitrate = Max_video_bitrate;
            settings.max_audio_bitrate = Max_audio_bitrate;
            settings.crf = Crf;

            this.Dispatcher.Dispatch(async () => {
                advanced.update(settings);
            });
        });
	}
    
	public AdvancedSettingsComponent()
	{
		InitializeComponent();
		BindingContext = this;

		Formats.Add("Loading");
		Codecs.Add("Loading");

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

			this.Dispatcher.Dispatch(() => {
				Formats = formats_as_collection;
				Codecs = codecs_as_collection;
				Output_format = advanced_settings.output_format;
				Preset = advanced_settings.preset;
				Max_quality = advanced_settings.max_quality;
				Codec = advanced_settings.codec;
				Max_video_bitrate = advanced_settings.max_video_bitrate;
				Max_audio_bitrate = advanced_settings.max_audio_bitrate;
				Crf = advanced_settings.crf;

                update_safety_lock = false;
			});
		});
	}
}