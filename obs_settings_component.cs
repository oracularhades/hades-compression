using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace HadesCompression;

public partial class ObsSettingsComponent : ContentView, INotifyPropertyChanged
{
    public bool update_safety_lock = true;
	private string _obs_address = null;
    public string Obs_address
    {
        get => _obs_address;
        set
        {
            if (_obs_address != value)
            {
                _obs_address = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private string _obs_password = null;
    public string Obs_password
    {
        get => _obs_password;
        set
        {
            if (_obs_password != value)
            {
                _obs_password = value;
				AdvancedSettingsUpdate();
                OnPropertyChanged();
            }
        }
    }

	private void AdvancedSettingsUpdate()
    {
        if (update_safety_lock == true)
        {
            return;
        }

        this.Dispatcher.Dispatch(async () => {
            Objects.advanced_settings settings = await advanced.get();
            settings.obs_address = Obs_address;
            settings.obs_password = Obs_password;

            Debug.WriteLine("Obs_passwordDD "+Obs_password);

            this.Dispatcher.Dispatch(async () => {
                advanced.update(settings);
            });
        });
	}
    
	public ObsSettingsComponent()
	{
		InitializeComponent();
		BindingContext = this;

		Task.Run(async () => {
			Objects.advanced_settings advanced_settings = await advanced.get();

			this.Dispatcher.Dispatch(() => {
				Obs_address = advanced_settings.obs_address;
				Obs_password = advanced_settings.obs_password;

                update_safety_lock = false;
			});
		});
	}
}