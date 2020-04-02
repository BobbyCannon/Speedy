#region References

using Speedy.Mobile.Services;
using Speedy.Mobile.Views;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile
{
	public partial class App : Application
	{
		#region Constructors

		public App()
		{
			InitializeComponent();

			DependencyService.Register<MockDataStore>();
			MainPage = new MainPage();
		}

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