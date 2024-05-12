using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class AdvancedSettingsGeneralComponent : ContentView, INotifyPropertyChanged
{
    public bool update_safety_lock = true;

	private int _you_have_uncompressed_files_threshold = 0;
    public int You_have_uncompressed_files_threshold
    {
        get => _you_have_uncompressed_files_threshold;
        set
        {
            if (_you_have_uncompressed_files_threshold != value)
            {
                _you_have_uncompressed_files_threshold = value;
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
            Objects.advanced_settings advanced_Settings = await advanced.get();
            advanced_Settings.you_have_uncompressed_files_reminder_threshold_GB = You_have_uncompressed_files_threshold;

            this.Dispatcher.Dispatch(async () => {
                advanced.update(advanced_Settings);
            });
        });
	}
    
	public AdvancedSettingsGeneralComponent()
	{
		InitializeComponent();
		BindingContext = this;

		Task.Run(async () => {
			Objects.advanced_settings advanced_settings = await advanced.get();

            You_have_uncompressed_files_threshold = advanced_settings.you_have_uncompressed_files_reminder_threshold_GB;

            update_safety_lock = false;
		});
	}
}