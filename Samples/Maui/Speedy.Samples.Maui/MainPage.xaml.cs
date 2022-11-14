namespace Speedy.Samples.Maui;

public partial class MainPage : ContentPage
{
	#region Fields

	private int count;

	#endregion

	#region Constructors

	public MainPage()
	{
		InitializeComponent();
	}

	#endregion

	#region Methods

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
		{
			CounterBtn.Text = $"Clicked {count} time";
		}
		else
		{
			CounterBtn.Text = $"Clicked {count} times";
		}

		SemanticScreenReader.Announce(CounterBtn.Text);
	}

	#endregion
}