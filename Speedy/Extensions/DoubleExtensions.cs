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

		#endregion
	}
}