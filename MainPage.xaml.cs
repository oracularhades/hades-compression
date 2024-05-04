using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace HadesCompression;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    // encoding_paused
    public ObservableCollection<String> input_directories { get; set; } = new ObservableCollection<String>();
    public ObservableCollection<String> output_directories { get; set; } = new ObservableCollection<String>();
    private bool automatically_compress_videos_last_value = false;
    private bool _automaticallyCompressVideos = true;
    public bool AutomaticallyCompressVideos
    {
        get => _automaticallyCompressVideos;
        set
        {
            if (_automaticallyCompressVideos != value)
            {
                _automaticallyCompressVideos = value;
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

    public MainPage()
    {
        InitializeComponent();

        BindingContext = this;

        Objects.get_hadescompression_directories directories = Files.get_hadescompression_directories();
        input_directories.Add(directories.input_directory);
        output_directories.Add(directories.output_directory);

        Task.Run(async () => Autocompress_Filehandling.start_watching());
        Task.Run(async () => {
            AutomaticallyCompressVideos = await Autocompress_Filehandling.get_automatically_compress_videos();
        });
        Task.Run(async () => {
            while (true) {
                Objects.obs_output obs_output = await Obs.get_obs_status(true, true);
                Debug.WriteLine($"OBS STREAMING: {obs_output.stream.outputActive}");

                if (obs_output.stream.outputActive == true)
                {
                    queue.pause_all();

                    EncodingPaused = true;
                    EncodingPausedMessage = "You're streaming, you've disabled encoding during stream to save resources. Encoding will continue when stream ends. Enjoy stream!";
                } else if (obs_output.recording.outputActive == true || obs_output.recording.outputPaused == true) {
                    queue.pause_all();

                    EncodingPaused = true;
                    EncodingPausedMessage = "You're recording, you've disabled encoding during recording to save resources. Encoding will continue when you finish recording.";
                } else if (obs_output.stream.outputActive == false) {
                    if (queue.was_paused == true) {
                        queue.resume_all();
                        queue.was_paused = false;

                        EncodingPaused = false;
                        EncodingPausedMessage = "";
                    }
                }
                Thread.Sleep(1000);
            }
        });
    }

    private void OnAutomaticallyCompressVideosToggled(object sender, ToggledEventArgs e)
    {
        bool value = e.Value;
        if (value == automatically_compress_videos_last_value) {
            // Stop infinite loop.
            return;
        }
        Task.Run(async () => {
            automatically_compress_videos_last_value = value;
            AutomaticallyCompressVideos = await Autocompress_Filehandling.set_automatically_compress_videos(value);
        });

        // AutomaticallyCompressVideos = false;
        // OnPropertyChanged();

        // if (count == 1)
        // 	CounterBtn.Text = $"Clicked {count} time";
        // else
        // 	CounterBtn.Text = $"Clicked {count} times";

        // SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Advanced_Page());
        Debug.WriteLine("OnCounterClicked");
        // AutomaticallyCompressVideos = false;
        // OnPropertyChanged();

        // if (count == 1)
        // 	CounterBtn.Text = $"Clicked {count} time";
        // else
        // 	CounterBtn.Text = $"Clicked {count} times";

        // SemanticScreenReader.Announce(CounterBtn.Text);
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
            Objects.get_hadescompression_directories relevant_directories = Files.get_hadescompression_directories();

            Objects.ffprobevideoinfo ffprobe = ffmpeg.ffprobe(result.FullPath);
            string output_directory = Files.is_valid_path(relevant_directories.output_directory+@"\"+Path.GetFileName(result.FullPath));
            Objects.encoding_config encoding_config_from_ffprobe = ffmpeg.get_encoding_config_from_ffprobe(ffprobe, output_directory);
            ffmpeg.encode(encoding_config_from_ffprobe, null);
        });
    }
}
