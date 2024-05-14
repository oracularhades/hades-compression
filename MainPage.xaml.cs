using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace HadesCompression;

public class MainPageBindingsStruct : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool new_update;
    public bool New_update
    {
        get => new_update;
        set
        {
            if (new_update != value)
            {
                new_update = value;
                OnPropertyChanged();
            }
        }
    }

    private bool show_uncompressed_files_to_delete_banner;
    public bool Show_uncompressed_files_to_delete_banner
    {
        get => show_uncompressed_files_to_delete_banner;
        set
        {
            if (show_uncompressed_files_to_delete_banner != value)
            {
                show_uncompressed_files_to_delete_banner = value;
                OnPropertyChanged();
            }
        }
    }

    private bool encodingPaused;
    public bool EncodingPaused
    {
        get => encodingPaused;
        set
        {
            if (encodingPaused != value)
            {
                encodingPaused = value;
                OnPropertyChanged();
            }
        }
    }

    private string encodingPausedMessage;
    public string EncodingPausedMessage
    {
        get => encodingPausedMessage;
        set
        {
            if (encodingPausedMessage != value)
            {
                encodingPausedMessage = value;
                OnPropertyChanged();
            }
        }
    }

    private string youve_saved;
    public string Youve_saved
    {
        get => youve_saved;
        set
        {
            if (youve_saved != value)
            {
                youve_saved = value;
                OnPropertyChanged();
            }
        }
    }

    private string input_directory;
    public string Input_directory
    {
        get => input_directory;
        set
        {
            if (input_directory != value)
            {
                input_directory = value;
                OnPropertyChanged();
            }
        }
    }

    private string output_directory;
    public string Output_directory
    {
        get => output_directory;
        set
        {
            if (output_directory != value)
            {
                output_directory = value;
                OnPropertyChanged();
            }
        }
    }

    private string hadescompression_will_use_a_max_of;
    public string Hadescompression_will_use_a_max_of
    {
        get => hadescompression_will_use_a_max_of;
        set
        {
            if (hadescompression_will_use_a_max_of != value)
            {
                hadescompression_will_use_a_max_of = value;
                OnPropertyChanged();
            }
        }
    }

    private int cpuLimit;
    public int CpuLimit
    {
        get => cpuLimit;
        set
        {
            if (cpuLimit != value)
            {
                cpuLimit = value;
                OnPropertyChanged();
            }
        }
    }

    private int memoryLimit;
    public int MemoryLimit
    {
        get => memoryLimit;
        set
        {
            if (memoryLimit != value)
            {
                memoryLimit = value;
                OnPropertyChanged();
            }
        }
    }

    private int threadLimit;
    public int ThreadLimit
    {
        get => threadLimit;
        set
        {
            if (threadLimit != value)
            {
                threadLimit = value;
                OnPropertyChanged();
            }
        }
    }

    private bool automaticallyCompressVideos;
    public bool AutomaticallyCompressVideos
    {
        get => automaticallyCompressVideos;
        set
        {
            if (automaticallyCompressVideos != value)
            {
                automaticallyCompressVideos = value;
                OnPropertyChanged();
            }
        }
    }

    private bool encodeWhileRecording;
    public bool EncodeWhileRecording
    {
        get => encodeWhileRecording;
        set
        {
            if (encodeWhileRecording != value)
            {
                encodeWhileRecording = value;
                OnPropertyChanged();
            }
        }
    }

    private bool encodeWhileStreaming;
    public bool EncodeWhileStreaming
    {
        get => encodeWhileStreaming;
        set
        {
            if (encodeWhileStreaming != value)
            {
                encodeWhileStreaming = value;
                OnPropertyChanged();
            }
        }
    }
}

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    // encoding_paused
    // Hadescompression_will_use_a_max_of
    public bool update_safety_lock = true;

    private MainPageBindingsStruct _MainPage_Bindings = new MainPageBindingsStruct{};
    public MainPageBindingsStruct MainPage_Bindings
    {
        get => _MainPage_Bindings;
        set
        {
            if (!Object.Equals(_MainPage_Bindings, _MainPage_Bindings))
            {
                _MainPage_Bindings = value;
                OnPropertyChanged(nameof(_MainPage_Bindings));
            }
        }
    }

    private void hades_compression_will_use()
    {
        MainPageBindingsStruct MainPage_Bindings_v = MainPage_Bindings;
        MainPage_Bindings_v.Hadescompression_will_use_a_max_of = $"Hades-compression will use a maximum of {MainPage_Bindings.CpuLimit * MainPage_Bindings.ThreadLimit}% CPU and {MainPage_Bindings.MemoryLimit * MainPage_Bindings.ThreadLimit}% memory at any given time.";
        MainPage_Bindings = MainPage_Bindings_v;
    }

    private async void update_settings() {
        if (update_safety_lock == true)
        {
            return;
        }

        hades_compression_will_use();

        Objects.settings settings = await Settings.get();
        settings.input_directory = MainPage_Bindings.Input_directory;
        settings.output_directory = MainPage_Bindings.Output_directory;
        settings.cpu_limit = MainPage_Bindings.CpuLimit;
        settings.memory_limit = MainPage_Bindings.MemoryLimit;
        settings.thread_limit = MainPage_Bindings.ThreadLimit;
        settings.automatically_compress_videos = MainPage_Bindings.AutomaticallyCompressVideos;
        settings.encode_while_recording = MainPage_Bindings.EncodeWhileRecording;
        settings.encode_while_streaming = MainPage_Bindings.EncodeWhileStreaming;

        this.Dispatcher.Dispatch(async () => {
            Settings.update(settings);
        });
    }

    private async void update_youve_saved()
    {
        Objects.stats stats = await Stats.get();

        MainPageBindingsStruct MainPage_Bindings_v = MainPage_Bindings;
        MainPage_Bindings_v.Youve_saved = $"You've saved {Files.formatted_output(stats.storage_saved_GB)} of space!";
        MainPage_Bindings = MainPage_Bindings_v;
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = MainPage_Bindings;

        Task.Run(async () => Autocompress_Filehandling.start_watching());
        Task.Run(async () => {
            Objects.settings settings = await Settings.get();

            MainPageBindingsStruct MainPage_Bindings_v = MainPage_Bindings;
            MainPage_Bindings_v.CpuLimit = settings.cpu_limit;
            MainPage_Bindings_v.MemoryLimit = settings.memory_limit;
            MainPage_Bindings_v.AutomaticallyCompressVideos = settings.automatically_compress_videos;
            MainPage_Bindings_v.ThreadLimit = settings.thread_limit;
            MainPage_Bindings_v.EncodeWhileRecording = settings.encode_while_recording;
            MainPage_Bindings_v.EncodeWhileStreaming = settings.encode_while_streaming;

            if (Settings.check_resource_threads_under_limit(settings.cpu_limit, settings.thread_limit) == false) {
                MainPage_Bindings_v.CpuLimit = Settings.max_cpu_resource_limit;
                MainPage_Bindings_v.ThreadLimit = 1;
            }
            if (Settings.check_resource_threads_under_limit(settings.memory_limit, settings.thread_limit) == false) {
                MainPage_Bindings_v.MemoryLimit = Settings.max_memory_resource_limit;
                MainPage_Bindings_v.ThreadLimit = 1;
            }

            Objects.get_hadescompression_directories directories = await Files.get_hadescompression_directories();
            MainPage_Bindings_v.Input_directory = directories.input_directory;
            MainPage_Bindings_v.Output_directory = directories.output_directory;

            MainPage_Bindings = MainPage_Bindings_v;

            hades_compression_will_use();

            update_safety_lock = false;
        });
        Task.Run(async () => {
            while (true) {
                update_youve_saved();
                
                MainPageBindingsStruct MainPage_Bindings_v = MainPage_Bindings;

                bool block_because_of_status = await Obs.block_because_of_status();
                if (block_because_of_status == false) {
                    // No condition has been met where we need to block right now, so no use running logic to decide text to display.

                    if (queue.was_paused == true) {
                        queue.resume_all();
                        queue.was_paused = false;

                        MainPage_Bindings_v.EncodingPaused = false;
                        MainPage_Bindings_v.EncodingPausedMessage = "";
                    }
                } else {
                    Objects.obs_output obs_output = await Obs.get_obs_status(true, true);

                    if (obs_output.stream.outputActive == true)
                    {
                        queue.pause_all();

                        MainPage_Bindings_v.EncodingPaused = true;
                        MainPage_Bindings_v.EncodingPausedMessage = "You're streaming, you've disabled encoding during stream to save resources. Encoding will continue when stream ends. Enjoy stream!";
                    } else if (obs_output.recording.outputActive == true || obs_output.recording.outputPaused == true) {
                        queue.pause_all();

                        MainPage_Bindings_v.EncodingPaused = true;
                        MainPage_Bindings_v.EncodingPausedMessage = "You're recording, you've disabled encoding during recording to save resources. Encoding will continue when you finish recording.";
                    }
                }
                
                MainPage_Bindings = MainPage_Bindings_v;
                
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
