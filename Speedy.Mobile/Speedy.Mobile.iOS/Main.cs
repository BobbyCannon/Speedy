#region References

using SQLitePCL;
using UIKit;

#endregion

namespace Speedy.Mobile.iOS
{
	public class Application
	{
		#region Methods

		// This is the main entry point of the application.
		private static void Main(string[] args)
		{
			Batteries_V2.Init();

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}

		#endregion
	}
}