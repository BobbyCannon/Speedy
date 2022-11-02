namespace Speedy.Extensions;

/// <summary>
/// Extensions for float
/// </summary>
public static class FloatExtensions
{
	#region Methods

	/// <summary>
	/// Decrement an float by a value or float.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be decremented. </param>
	/// <param name="decrease"> An optional value to decrement. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static float Decrement(this float value, float decrease = float.Epsilon)
	{
		if (float.IsInfinity(value) || float.IsInfinity(decrease))
		{
			return value;
		}

		if (float.IsNaN(value) || float.IsNaN(decrease))
		{
			return value;
		}

		return value - decrease;
	}

	/// <summary>
	/// Increment an float by a value or float.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be incremented. </param>
	/// <param name="increase"> An optional increase. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static float Increment(this float value, float increase = float.Epsilon)
	{
		if (float.IsInfinity(value) || float.IsInfinity(increase))
		{
			return value;
		}

		if (float.IsNaN(value) || float.IsNaN(increase))
		{
			return value;
		}

		return value + increase;
	}

	#endregion
}