namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class StringExtensions
	{
		#region Methods

		/// <summary>
		/// Trims string to a maximum length.
		/// </summary>
		/// <param name="value"> The value to process. </param>
		/// <param name="max"> The maximum length of the string. </param>
		/// <param name="addEllipses"> The option to add ellipses to shorted strings. Defaults to false. </param>
		/// <returns> The value limited to the maximum length. </returns>
		public static string MaxLength(this string value, int max, bool addEllipses = false)
		{
			if (string.IsNullOrWhiteSpace(value) || max <= 0)
			{
				return string.Empty;
			}

			var shouldAddEllipses = addEllipses && value.Length > max && max >= 4;

			return value.Length > max ? value.Substring(0, shouldAddEllipses ? max - 3 : max) + (shouldAddEllipses ? "..." : string.Empty) : value;
		}

		#endregion
	}
}