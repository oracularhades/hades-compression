using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HadesCompression
{
    public partial class QueueComponent : ContentView, INotifyPropertyChanged
    {
        private int _queue_count = 0;
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

        private List<Objects.queue_item> _queue_bind = new List<Objects.queue_item>();
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
        private async void get_queue()
        {
            while (true)
            {
                Thread.Sleep(1000);
                List<Objects.queue_item> queue_items = await queue.get();

                if (!object.Equals(queue_bind, queue_items)) {
                    queue_count = queue_items.Count;
                    queue_bind = queue_items;
                }
            }
        }
        public QueueComponent()
        {
            InitializeComponent();

            BindingContext = this;

            Task.Run(async () => get_queue());
        }
    }
}