namespace Speedy.Samples.Xamarin
{
	public partial class MainPage
	{
		#region Constructors

		public MainPage(MainViewModel mainViewModel)
		{
			InitializeComponent();

			ViewModel = mainViewModel;
			BindingContext = ViewModel;
		}

		#endregion

		#region Properties

		public MainViewModel ViewModel { get; }

		#endregion
	}
}