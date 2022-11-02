#region References

using System;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for the Random type.
/// </summary>
public static class RandomExtensions
{
	#region Methods

	/// <summary>
	/// Returns a random decimal floating point that is within a specified range.
	/// </summary>
	/// <param name="random"> The random object to use. </param>
	/// <param name="minimum"> The inclusive lower bound of the random number returned. </param>
	/// <param name="maximum"> The exclusive maximum bound of the random number returned. </param>
	/// <param name="scale"> The scale of the decimal. How precise? 1 = 0.1, 2 = 0.01 </param>
	/// <returns>
	/// A decimal number greater than or equal to minValue and less than maxValue; that is, the
	/// range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.
	/// </returns>
	public static decimal NextDecimal(this Random random, decimal minimum = 0, decimal maximum = decimal.MaxValue, byte scale = 0)
	{
		if (maximum <= minimum)
		{
			return minimum;
		}

		var result = ((decimal) random.NextDouble() * (maximum - minimum)) + minimum;
		return Math.Round(result, scale);
	}

	/// <summary>
	/// Returns a random double floating point that is within a specified range.
	/// </summary>
	/// <param name="random"> The random object to use. </param>
	/// <param name="minimum"> The inclusive lower bound of the random number returned. </param>
	/// <param name="maximum"> The exclusive maximum bound of the random number returned. </param>
	/// <param name="scale"> The scale of the double. How precise? 1 = 0.1, 2 = 0.01 </param>
	/// <returns>
	/// A double floating point number greater than or equal to minValue and less than maxValue; that is, the
	/// range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.
	/// </returns>
	public static double NextDouble(this Random random, double minimum = 0, double maximum = double.MaxValue, byte scale = 0)
	{
		if (maximum <= minimum)
		{
			return minimum;
		}

		var result = (random.NextDouble() * (maximum - minimum)) + minimum;
		return Math.Round(result, scale);
	}

	#endregion
}