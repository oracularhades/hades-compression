using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;

namespace HadesCompression;

public partial class UncompressedFilesToDelete : ContentView, INotifyPropertyChanged
{
    private bool _Show_uncompressed_files_to_delete_banner = false;
    public bool Show_uncompressed_files_to_delete_banner
    {
        get => _Show_uncompressed_files_to_delete_banner;
        set
        {
            if (_Show_uncompressed_files_to_delete_banner != value)
            {
                _Show_uncompressed_files_to_delete_banner = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Show_uncompressed_files_header = null;
    public string Show_uncompressed_files_header
    {
        get => _Show_uncompressed_files_header;
        set
        {
            if (_Show_uncompressed_files_header != value)
            {
                _Show_uncompressed_files_header = value;
                OnPropertyChanged();
            }
        }
    }

    private string _Show_uncompressed_files_description = null;
    public string Show_uncompressed_files_description
    {
        get => _Show_uncompressed_files_description;
        set
        {
            if (_Show_uncompressed_files_description != value)
            {
                _Show_uncompressed_files_description = value;
                OnPropertyChanged();
            }
        }
    }

    private Page FindParentPage(Element element)
    {
        if (element == null)
            return null;

        if (element is Page page)
            return page;

        return FindParentPage(element.Parent);
    }

    private async void OnDeleteOriginalUncompressedClicked(object sender, EventArgs e)
    {
        Page parentPage = FindParentPage(this);
        if (parentPage != null)
        {
            Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
            string input_directory_processed = relevant_directories.input_directory_processed;

            bool answer = await parentPage.DisplayAlert("Delete orignal, uncompressed files?", $"Please make sure your output files (from Hades-compression/FFmpeg) are exactly as you expect them to be. Video processing is never perfect, there is a small chance there are FPS, audio, quality, etc issues\n\nFiles in {input_directory_processed} will be deleted", "Delete", "Cancel");
            if (answer == true)
            {

                Files.delete_files_in_directory(input_directory_processed);
                Show_uncompressed_files_to_delete_banner = false;
            }
        }
    }

    private async void OnViewFolderClicked(object sender, EventArgs e)
    {
        Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
        string input_directory_processed = relevant_directories.input_directory_processed;

        Files.open_folder(input_directory_processed);
    }

    private async void get_processed_size()
    {
        while (true)
        {
            Debug.WriteLine("get_processed_size");
            Thread.Sleep(1000);

            Objects.get_hadescompression_directories relevant_directories = await Files.get_hadescompression_directories();
            string input_directory_processed = relevant_directories.input_directory_processed;

            long size = Files.get_directory_size(input_directory_processed);
            string formatted_output = Files.formatted_output(size);

            Objects.advanced_settings advanced_settings = await advanced.get();
            Debug.WriteLine($"{size} | {Files.bytes_to_GB(size)} > {advanced_settings.you_have_uncompressed_files_reminder_threshold_GB}");
            if (Files.bytes_to_GB(size) >= advanced_settings.you_have_uncompressed_files_reminder_threshold_GB)
            {
                Show_uncompressed_files_to_delete_banner = true;
                Show_uncompressed_files_header = $"{formatted_output} of uncompressed files";
                Show_uncompressed_files_description = $"You have {formatted_output} of unencoded, original files in {input_directory_processed} - you should check your output files are as expected and delete your original files to save space. The threshold for this reminder can be changed in settings.";
            } else {
                Show_uncompressed_files_to_delete_banner = false;
            }
        }
    }
    
	public UncompressedFilesToDelete()
	{
		InitializeComponent();
		BindingContext = this;

        Task.Run(async () => {
            get_processed_size();
        });
	}
}