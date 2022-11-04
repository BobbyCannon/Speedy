#region References

using Foundation;
using UIKit;

#endregion

[assembly: Preserve]
namespace Speedy.Samples.Xamarin.iOS;

public class Application
{
	#region Methods

	// This is the main entry point of the application.
	private static void Main(string[] args)
	{
		// if you want to use a different Application Delegate class from "AppDelegate"
		// you can specify it here.
		UIApplication.Main(args, null, typeof(AppDelegate));
	}

	#endregion
}