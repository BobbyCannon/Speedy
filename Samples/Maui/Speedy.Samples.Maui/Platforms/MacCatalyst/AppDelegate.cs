#region References

using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Samples.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	#region Methods

	protected override MauiApp CreateMauiApp()
	{
		return MauiProgram.CreateMauiApp();
	}

	#endregion
}