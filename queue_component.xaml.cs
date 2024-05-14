using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HadesCompression
{
    public class QueueDataStruct : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _queue_count;
        public int queue_count
        {
            get => _queue_count;
            set
            {
                if (_queue_count != value)
                {
                    _queue_count = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<Objects.queue_item> _queue_bind;
        public List<Objects.queue_item> queue_bind
        {
            get => _queue_bind;
            set
            {
                if (_queue_bind != value)
                {
                    _queue_bind = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public partial class QueueComponent : ContentView, INotifyPropertyChanged
    {
        private QueueDataStruct queue_data = new QueueDataStruct{};

        private async void get_queue()
        {
            while (true)
            {
                Thread.Sleep(2500);
                List<Objects.queue_item> queue_items = await queue.get();

                if (!object.Equals(queue_data.queue_bind, queue_items)) {
                    QueueDataStruct queue_data_v = queue_data;
                    queue_data_v.queue_count = queue_items.Count;
                    queue_data_v.queue_bind = queue_items;
                    queue_data = queue_data_v;
                }
            }
        }
        public QueueComponent()
        {
            InitializeComponent();

            BindingContext = queue_data;

            Task.Run(async () => get_queue());
        }
    }
}