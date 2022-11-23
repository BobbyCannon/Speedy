#region References

#if ANDROID
using Android.App;
#endif

#endregion

namespace Speedy.Application.Maui;

/// <summary>
/// Maui platform specific implementations.
/// </summary>
public static class MauiPlatform
{
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
	/// Initialize the xamarin platform for Android.
	/// </summary>
	/// <param name="activity"> The activity. </param>
	public static void Initialize(Activity activity)
	{
		MainActivity = activity;
	}

	#endif

	#endregion
}