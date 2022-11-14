#region References

using Android.App;
using Android.Runtime;

#endregion

namespace Speedy.Samples.Maui;

[Application]
public class MainApplication : MauiApplication
{
	#region Constructors

	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	#endregion

	#region Methods

	protected override MauiApp CreateMauiApp()
	{
		return MauiProgram.CreateMauiApp();
	}

	#endregion
}