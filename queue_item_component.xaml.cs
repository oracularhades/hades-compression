using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace HadesCompression
{
    public partial class QueueItemComponent : ContentView
    {
        public QueueItemComponent()
        {
            InitializeComponent();

            BindingContext = this;
        }

        private void OnQueuePlayClicked(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            var queue_item = (Objects.queue_item)button.CommandParameter;

            string path = queue_item.path;

            Task.Run(async () => ffmpeg.play(path));
        }

        private void OnQueuePauseClicked(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            var queue_item = (Objects.queue_item)button.CommandParameter;

            string path = queue_item.path;
            Task.Run(async () => ffmpeg.pause(path, queue_item));
        }
    }
}