#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for enumerations
	/// </summary>
	public static class EnumExtensions
	{
		#region Methods

		/// <summary>
		/// Clear the "flagged" enum value.
		/// </summary>
		/// <typeparam name="T"> The type of the enum value. </typeparam>
		/// <param name="value"> The value to update. </param>
		/// <param name="flag"> The flag to be cleared. </param>
		/// <returns> The value with the flagged cleared. </returns>
		public static T ClearFlag<T>(this T value, T flag) where T : Enum
		{
			return value.SetFlag(flag, false);
		}

		/// <summary>
		/// Set the "flagged" enum value.
		/// </summary>
		/// <typeparam name="T"> The type of the enum value. </typeparam>
		/// <param name="value"> The value to update. </param>
		/// <param name="flag"> The flag to be set. </param>
		/// <returns> The value with the flagged set. </returns>
		public static T SetFlag<T>(this T value, T flag) where T : Enum
		{
			return value.SetFlag(flag, true);
		}

		private static T SetFlag<T>(this T value, T flag, bool set) where T : Enum
		{
			var eValue = Convert.ToUInt64(value);
			var eFlag = Convert.ToUInt64(flag);
			var fValue = set ? eValue | eFlag : eValue & ~eFlag;
			return (T) Enum.ToObject(typeof(T), fValue);
		}

		#endregion
	}
}