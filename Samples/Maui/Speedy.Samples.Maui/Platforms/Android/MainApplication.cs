#region References

using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

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