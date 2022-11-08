#region References

#if MONOANDROID
using Android.App;
#endif

#endregion

namespace Speedy.Application.Xamarin
{
	/// <summary>
	/// Xamarin platform specific implementations.
	/// </summary>
	public static class XamarinPlatform
	{
		#region Properties

		#if MONOANDROID

		/// <summary>
		/// The main activity for Android platform.
		/// </summary>
		public static Activity MainActivity { get; private set; }

		#endif

		#endregion

		#region Methods

		#if MONOANDROID

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
}