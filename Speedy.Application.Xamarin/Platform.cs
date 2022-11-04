#region References

#if MONOANDROID
using Android.App;
#endif

#endregion

namespace Speedy.Application.Xamarin
{
	public static class XamarinPlatform
	{
		#region Properties

		#if MONOANDROID
		public static Activity MainActivity { get; private set; }

		#endif

		#endregion

		#region Methods

		#if MONOANDROID
		public static void Initialize(Activity activity)
		{
			MainActivity = activity;
		}

		#endif

		#endregion
	}
}