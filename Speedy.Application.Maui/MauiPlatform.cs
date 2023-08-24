#region References
#if ANDROID
using Android.App;
#endif

#if WINDOWS
using System.Diagnostics;
using Microsoft.Maui.Handlers;
#endif
#endregion

namespace Speedy.Application.Maui;

/// <summary>
/// Maui platform specific implementations.
/// </summary>
public static class MauiPlatform
{
	#region Constructors

	static MauiPlatform()
	{
		#if WINDOWS
		PickerHandler.Mapper.Add(nameof(View.HorizontalOptions), MapHorizontalOptions);
		#endif
	}

	#endregion

	#region Properties

	#if ANDROID

	/// <summary>
	/// The main activity for Android platform.
	/// </summary>
	public static Activity MainActivity { get; private set; }

	#endif

	#endregion

	#region Methods

	#if ANDROID

	/// <summary>
	/// Initialize the Maui platform for Android.
	/// </summary>
	/// <param name="activity"> The activity. </param>
	public static void Initialize(Activity activity)
	{
		MainActivity = activity;
	}

	#endif

	#if WINDOWS

	/// <summary>
	/// Initialize the Maui platform for Windows.
	/// </summary>
	public static void Initialize()
	{
	}

	private static void MapHorizontalOptions(IViewHandler handler, IView view)
	{
		if (view is not View mauiView)
		{
			return;
		}

		if (handler.PlatformView is not Microsoft.UI.Xaml.FrameworkElement element)
		{
			return;
		}

		element.HorizontalAlignment = mauiView.HorizontalOptions.Alignment switch
		{
			LayoutAlignment.Start => Microsoft.UI.Xaml.HorizontalAlignment.Left,
			LayoutAlignment.Center => Microsoft.UI.Xaml.HorizontalAlignment.Center,
			LayoutAlignment.End => Microsoft.UI.Xaml.HorizontalAlignment.Right,
			LayoutAlignment.Fill => Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
			_ => Microsoft.UI.Xaml.HorizontalAlignment.Left
		};
	}

	#endif

	#endregion
}