using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace HadesCompression;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    // encoding_paused
    // Hadescompression_will_use_a_max_of
    public bool update_safety_lock = true;
    private string _youve_saved = "You've saved 0KB of space!";
    public string Youve_saved
    {
        get => _youve_saved;
        set
        {
            if (_youve_saved != value)
            {
                _youve_saved = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Hadescompression_will_use_a_max_of = "";
    public string Hadescompression_will_use_a_max_of
    {
        get => _Hadescompression_will_use_a_max_of;
        set
        {
            if (_Hadescompression_will_use_a_max_of != value)
            {
                _Hadescompression_will_use_a_max_of = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Input_directory = null;
    public string Input_directory
    {
        get => _Input_directory;
        set
        {
            if (_Input_directory != value)
            {
                _Input_directory = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private string _Output_directory = null;
    public string Output_directory
    {
        get => _Output_directory;
        set
        {
            if (_Output_directory != value)
            {
                _Output_directory = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private int _CpuLimit;
    public int CpuLimit
    {
        get => _CpuLimit;
        set
        {
            if (_CpuLimit != value)
            {
                _CpuLimit = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private int _MemoryLimit;
    public int MemoryLimit
    {
        get => _MemoryLimit;
        set
        {
            if (_MemoryLimit != value)
            {
                _MemoryLimit = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private int _ThreadLimit;
    public int ThreadLimit
    {
        get => _ThreadLimit;
        set
        {
            if (_ThreadLimit != value)
            {
                _ThreadLimit = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private bool _automaticallyCompressVideos = true;
    public bool AutomaticallyCompressVideos
    {
        get => _automaticallyCompressVideos;
        set
        {
            if (_automaticallyCompressVideos != value)
            {
                _automaticallyCompressVideos = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private bool _encodeWhileRecording = false;
    public bool EncodeWhileRecording
    {
        get => _encodeWhileRecording;
        set
        {
            if (_encodeWhileRecording != value)
            {
                _encodeWhileRecording = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private bool _encodeWhileStreaming = false;
    public bool EncodeWhileStreaming
    {
        get => _encodeWhileStreaming;
        set
        {
            if (_encodeWhileStreaming != value)
            {
                _encodeWhileStreaming = value;
                update_settings();
                OnPropertyChanged();
            }
        }
    }
    private bool _EncodingPaused = false;
    public bool EncodingPaused
    {
        get => _EncodingPaused;
        set
        {
            if (_EncodingPaused != value)
            {
                _EncodingPaused = value;
                OnPropertyChanged();
            }
        }
    }

    private string _EncodingPausedMessage = "";
    public string EncodingPausedMessage
    {
        get => _EncodingPausedMessage;
        set
        {
            if (_EncodingPausedMessage != value)
            {
                _EncodingPausedMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private void hades_compression_will_use()
    {
        Hadescompression_will_use_a_max_of = $"Hades-compression will use a maximum of {CpuLimit * ThreadLimit}% CPU and {MemoryLimit * ThreadLimit}% at any given time.";
    }

    private async void update_settings() {
        if (update_safety_lock == true)
        {
            return;
        }

        hades_compression_will_use();

        Objects.settings settings = await Settings.get();
        settings.input_directory = Input_directory;
        settings.output_directory = Output_directory;
        settings.cpu_limit = CpuLimit;
        settings.memory_limit = MemoryLimit;
        settings.thread_limit = ThreadLimit;
        settings.automatically_compress_videos = AutomaticallyCompressVideos;
        settings.encode_while_recording = EncodeWhileRecording;
        settings.encode_while_streaming = EncodeWhileStreaming;

        this.Dispatcher.Dispatch(async () => {
            Settings.update(settings);
        });
    }

    private async void update_youve_saved()
    {
        Objects.stats stats = await Stats.get();
        Youve_saved = $"You've saved {Files.formatted_output(stats.storage_saved_GB)} of space!";
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        Task.Run(async () => Autocompress_Filehandling.start_watching());
        Task.Run(async () => {
            Objects.settings settings = await Settings.get();
            CpuLimit = settings.cpu_limit;
            MemoryLimit = settings.memory_limit;
            AutomaticallyCompressVideos = settings.automatically_compress_videos;
            ThreadLimit = settings.thread_limit;
            EncodeWhileRecording = settings.encode_while_recording;
            EncodeWhileStreaming = settings.encode_while_streaming;
            hades_compression_will_use();

            update_safety_lock = false;
        });
        Task.Run(async () => {
            Objects.get_hadescompression_directories directories = await Files.get_hadescompression_directories();
            Input_directory = directories.input_directory;
            Output_directory = directories.output_directory;
        });
        Task.Run(async () => {
            while (true) {
                update_youve_saved();
                
                bool block_because_of_status = await Obs.block_because_of_status();
                Debug.WriteLine("block_because_of_status "+block_because_of_status);
                if (block_because_of_status == false) {
                    // No condition has been met where we need to block right now, so no use running logic to decide text to display.

                    if (queue.was_paused == true) {
                        queue.resume_all();
                        queue.was_paused = false;

                        EncodingPaused = false;
                        EncodingPausedMessage = "";
                    }
                } else {
                    Objects.obs_output obs_output = await Obs.get_obs_status(true, true);

                    if (obs_output.stream.outputActive == true)
                    {
                        queue.pause_all();

                        EncodingPaused = true;
                        EncodingPausedMessage = "You're streaming, you've disabled encoding during stream to save resources. Encoding will continue when stream ends. Enjoy stream!";
                    } else if (obs_output.recording.outputActive == true || obs_output.recording.outputPaused == true) {
                        queue.pause_all();

                        EncodingPaused = true;
                        EncodingPausedMessage = "You're recording, you've disabled encoding during recording to save resources. Encoding will continue when you finish recording.";
                    }
                }
                Thread.Sleep(200);
            }
        });
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Advanced_Page());
        Debug.WriteLine("OnCounterClicked");
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync();
        if (result == null)
        {
            return;
        }

        Task.Run(async () => {
            Debug.WriteLine("START FFMPEG");
            Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();

            Objects.ffprobevideoinfo ffprobe = ffmpeg.ffprobe(result.FullPath);
            string output_directory = Files.is_valid_path(relevant_directories.output_directory+@"\"+Path.GetFileName(result.FullPath));
            Objects.encoding_config encoding_config_from_ffprobe = ffmpeg.get_encoding_config_from_ffprobe(ffprobe, output_directory);
            ffmpeg.encode(encoding_config_from_ffprobe, null);
        });
    }
}
