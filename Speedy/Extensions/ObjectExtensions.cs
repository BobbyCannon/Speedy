#region References

using System;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class ObjectExtensions
	{
		#region Methods

		/// <summary>
		/// Executes a provided action if the test is successful.
		/// </summary>
		/// <param name="test"> The test to determine action to take. </param>
		/// <param name="action1"> The action to perform if the test is true. </param>
		/// <param name="action2"> The action to perform if the test is false. </param>
		public static void IfThenElse(Func<bool> test, Action action1, Action action2)
		{
			if (test())
			{
				action1();
			}
			else
			{
				action2();
			}
		}

		/// <summary>
		/// Validate an object as a string with in a range.
		/// </summary>
		/// <param name="value"> The string as an object. </param>
		/// <param name="expected"> The expected value. </param>
		/// <returns> True if the value is a boolean and matches the expected value. </returns>
		public static bool ValidateBoolean(this object value, bool expected)
		{
			return value is bool bValue && (bValue == expected);
		}

		/// <summary>
		/// Validate an object as a string with in a range.
		/// </summary>
		/// <param name="value"> The string as an object. </param>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <returns> True if the value is a string and within the provided range. </returns>
		public static bool ValidateIntRange(this object value, int minimum, int maximum)
		{
			if (value is not int iValue)
			{
				return false;
			}

			return (iValue >= minimum)
				&& (iValue <= maximum);
		}

		/// <summary>
		/// Validate an object as a string with in a range.
		/// </summary>
		/// <param name="value"> The string as an object. </param>
		/// <param name="minimum"> The inclusive minimum value. </param>
		/// <param name="maximum"> The inclusive maximum value. </param>
		/// <returns> True if the value is a string and within the provided range. </returns>
		public static bool ValidateStringRange(this object value, int minimum, int maximum)
		{
			if (value is not string sValue)
			{
				return false;
			}

			return (sValue.Length >= minimum)
				&& (sValue.Length <= maximum);
		}

		#endregion
	}
}