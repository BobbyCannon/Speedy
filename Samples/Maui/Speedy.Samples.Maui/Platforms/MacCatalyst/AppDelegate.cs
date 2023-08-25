using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Speedy.Samples.Maui
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		#region Methods

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		#endregion
	}
}