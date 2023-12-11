#region References

using UIKit;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Samples.Maui;

public class Program
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