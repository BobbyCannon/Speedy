#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for double
	/// </summary>
	public static class DoubleExtensions
	{
		#region Methods

		/// <summary>
		/// Decrement an double by a value or double.Epsilon if not provided.
		/// </summary>
		/// <param name="value"> The value to be decremented. </param>
		/// <param name="decrease"> An optional value to decrement. The value defaults to the smallest possible value. </param>
		/// <returns> The incremented value. </returns>
		public static double Decrement(this double value, double decrease = double.Epsilon)
		{
			if (double.IsInfinity(value) || double.IsInfinity(decrease))
			{
				return value;
			}

			if (double.IsNaN(value) || double.IsNaN(decrease))
			{
				return value;
			}

			return value - decrease;
		}

		/// <summary>
		/// Increment an double by a value or double.Epsilon if not provided.
		/// </summary>
		/// <param name="value"> The value to be incremented. </param>
		/// <param name="increase"> An optional increase. The value defaults to the smallest possible value. </param>
		/// <returns> The incremented value. </returns>
		public static double Increment(this double value, double increase = double.Epsilon)
		{
			if (double.IsInfinity(value) || double.IsInfinity(increase))
			{
				return value;
			}

			if (double.IsNaN(value) || double.IsNaN(increase))
			{
				return value;
			}

			return value + increase;
		}

		/// <summary>
		/// Convert Fahrenheit to Celsius.
		/// </summary>
		/// <param name="fahrenheit"> The temperature in Fahrenheit. </param>
		/// <returns> The temperature in Celsius. </returns>
		public static decimal ToCelsius(this decimal fahrenheit)
		{
			return Math.Round((fahrenheit - 32) / 1.8m, 2);
		}

		/// <summary>
		/// Convert Celsius to Fahrenheit.
		/// </summary>
		/// <param name="celsius"> The temperature in Celsius. </param>
		/// <returns> The temperature in Fahrenheit. </returns>
		public static float ToFahrenheit(this float celsius)
		{
			return (float) Math.Round((celsius * 1.8f) + 32, 2);
		}

		/// <summary>
		/// Convert Celsius to Fahrenheit.
		/// </summary>
		/// <param name="celsius"> The temperature in Celsius. </param>
		/// <returns> The temperature in Fahrenheit. </returns>
		public static decimal ToFahrenheit(this decimal celsius)
		{
			return Math.Round((celsius * 1.8m) + 32, 2);
		}

		/// <summary>
		/// Convert kilometers to miles per hour.
		/// </summary>
		/// <param name="kilometersPerHour"> The speed in kilometers per hour. </param>
		/// <returns> The speed in miles per hour. </returns>
		public static float ToMilesPerHour(this float kilometersPerHour)
		{
			return (float) ToMilesPerHour((decimal) kilometersPerHour);
		}

		/// <summary>
		/// Convert kilometers to miles per hour.
		/// </summary>
		/// <param name="kilometersPerHour"> The speed in kilometers per hour. </param>
		/// <returns> The speed in miles per hour. </returns>
		public static double ToMilesPerHour(this double kilometersPerHour)
		{
			return (double) ToMilesPerHour((decimal) kilometersPerHour);
		}

		/// <summary>
		/// Convert kilometers to miles per hour.
		/// </summary>
		/// <param name="kilometersPerHour"> The speed in kilometers per hour. </param>
		/// <returns> The speed in miles per hour. </returns>
		public static decimal ToMilesPerHour(this decimal kilometersPerHour)
		{
			return kilometersPerHour / 1.6093m;
		}

		#endregion
	}
}