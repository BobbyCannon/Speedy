#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents the service to provide time. Allows control for when the system is being tested.
	/// </summary>
	public static class TimeService
	{
		#region Constructors

		static TimeService()
		{
			Reset();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the date time in the format of the current time zone.
		/// </summary>
		public static DateTime Now => NowProvider();

		/// <summary>
		/// Gets or sets the Now time provider. Should be in the correct current time zone format.
		/// </summary>
		public static Func<DateTime> NowProvider { get; set; }

		/// <summary>
		/// Gets the date time in the format of UTC time zone.
		/// </summary>
		public static DateTime UtcNow => UtcNowProvider();

		/// <summary>
		/// Gets or sets the UTC time provider.
		/// </summary>
		public static Func<DateTime> UtcNowProvider { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Resets the providers to the default values.
		/// </summary>
		public static void Reset()
		{
			UtcNowProvider = () => DateTime.UtcNow;
			NowProvider = () => DateTime.Now;
		}

		#endregion
	}
}