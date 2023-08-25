namespace Speedy.Samples.Maui;

public partial class MainPage
{
	#region Constructors

	public MainPage(MainViewModel model)
	{
		InitializeComponent();

		ViewModel = model;
		BindingContext = model;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion
}