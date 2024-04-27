namespace Hades_Compression;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		Console.WriteLine("OnCounterClicked");
		count++;

		// if (count == 1)
		// 	CounterBtn.Text = $"Clicked {count} time";
		// else
		// 	CounterBtn.Text = $"Clicked {count} times";

		// SemanticScreenReader.Announce(CounterBtn.Text);
	}

	private async void OnPickFileClicked(object sender, EventArgs e)
	{
		await DisplayAlert("Alert", "This is an alert message.", "OK");

		// var options = new PickOptions
		// {
		// 	PickerTitle = "Please select a file to compress"
		// };

		// var result = await FilePicker.Default.PickAsync();
		// if (result != null)
		// {
		// 	// Handle the file here
		// }
	}
}

