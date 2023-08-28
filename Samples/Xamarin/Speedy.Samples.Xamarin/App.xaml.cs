#region References

using Speedy.Application.Xamarin;

#endregion

namespace Speedy.Samples.Xamarin
{
	public partial class App
	{
		#region Constructors

		public App()
		{
			InitializeComponent();

			var dispatcher = new XamarinDispatcher();
			MainViewModel = new MainViewModel(dispatcher);
			MainPage = new MainPage(MainViewModel);
		}

		#endregion

		#region Properties

		public MainViewModel MainViewModel { get; }

		#endregion

		#region Methods

		protected override void OnResume()
		{
		}

		protected override void OnSleep()
		{
		}

		protected override void OnStart()
		{
		}

		#endregion
	}
}