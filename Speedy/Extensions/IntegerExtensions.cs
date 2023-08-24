namespace Speedy.Extensions;

/// <summary>
/// Extensions for numbers
/// </summary>
public static class IntegerExtensions
{
	#region Methods

	/// <summary>
	/// Ensure the value falls between the ranges.
	/// </summary>
	/// <param name="value"> The nullable float value. </param>
	/// <param name="min"> The inclusive minimal value. </param>
	/// <param name="max"> The inclusive maximum value. </param>
	/// <returns> The value within the provided ranges. </returns>
	public static int EnsureRange(this int? value, int min, int max)
	{
		return value is null ? min : EnsureRange(value.Value, min, max);
	}

	/// <summary>
	/// Ensure the value falls between the ranges.
	/// </summary>
	/// <param name="value"> The nullable int value. </param>
	/// <param name="min"> The inclusive minimal value. </param>
	/// <param name="max"> The inclusive maximum value. </param>
	/// <returns> The value within the provided ranges. </returns>
	public static int EnsureRange(this int value, int min, int max)
	{
		return value < min ? min : value > max ? max : value;
	}

	/// <summary>
	/// Determines if a value represents an even integer value.
	/// </summary>
	/// <param name="value"> The value to be checked. </param>
	/// <returns> True if value is an even integer otherwise false. </returns>
	public static bool IsEven(this int value)
	{
		#if (NET7_0_OR_GREATER)
		return int.IsEvenInteger(value);
		#else
		return (value & 1) == 0;
		#endif
	}

	/// <summary>
	/// Determines if a value represents an odd integer value.
	/// </summary>
	/// <param name="value"> The value to be checked. </param>
	/// <returns> True if value is an odd integer otherwise false. </returns>
	public static bool IsOdd(this int value)
	{
		#if (NET7_0_OR_GREATER)
		return int.IsOddInteger(value);
		#else
		return (value & 1) != 0;
		#endif
	}

	#endregion
}