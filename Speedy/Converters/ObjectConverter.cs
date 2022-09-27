#region References

using System;
using System.ComponentModel;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Converters
{
	/// <summary>
	/// Converts an object to different types
	/// </summary>
	public static class ObjectConverter
	{
		#region Methods

		/// <summary>
		/// Converts the object to a byte value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a byte format or returns the default value. </returns>
		public static byte ToByte(this object value)
		{
			return value is byte castValue ? castValue : Convert.ToByte(value);
		}

		/// <summary>
		/// Converts the object to a char value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a char format or returns the default value. </returns>
		public static char ToChar(this object value)
		{
			return value is char castValue ? castValue : Convert.ToChar(value);
		}

		/// <summary>
		/// Converts the object to a DateTime value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a DateTime format or returns the default value. </returns>
		public static DateTime ToDateTime(this object value)
		{
			return value is DateTime dateTime ? dateTime : Convert.ToDateTime(value);
		}

		/// <summary>
		/// Converts the object to a DateTimeOffset value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a DateTimeOffset format or returns the default value. </returns>
		public static DateTimeOffset ToDateTimeOffset(this object value)
		{
			return value is DateTimeOffset offsetValue
				? offsetValue
				: new DateTimeOffsetConverter().ConvertFrom(value) is DateTimeOffset offset
					? offset
					: default;
		}

		/// <summary>
		/// Converts the object to a decimal value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a decimal format or returns the default value. </returns>
		public static decimal ToDecimal(this object value)
		{
			return value is decimal castValue ? castValue : Convert.ToDecimal(value);
		}

		/// <summary>
		/// Converts the object to a double value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a double format or returns the default value. </returns>
		public static double ToDouble(this object value)
		{
			return value is double castValue ? castValue : Convert.ToDouble(value);
		}

		/// <summary>
		/// Converts the object to a float value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a float format or returns the default value. </returns>
		public static float ToFloat(this object value)
		{
			return value is float castValue ? castValue : Convert.ToSingle(value);
		}

		/// <summary>
		/// Converts the object to a Int16 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a Int16 format or returns the default value. </returns>
		public static short ToInt16(this object value)
		{
			return value is short castValue ? castValue : Convert.ToInt16(value);
		}

		/// <summary>
		/// Converts the object to a Int32 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a Int32 format or returns the default value. </returns>
		public static int ToInt32(this object value)
		{
			return value is int castValue ? castValue : Convert.ToInt32(value);
		}

		/// <summary>
		/// Converts the object to a Int64 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a Int64 format or returns the default value. </returns>
		public static long ToInt64(this object value)
		{
			return value is long castValue ? castValue : Convert.ToInt64(value);
		}

		/// <summary>
		/// Converts the object to a DateTime value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a DateTime format or returns the default value. </returns>
		public static OscTimeTag ToOscTimeTag(this object value)
		{
			return value is OscTimeTag dateTime ? dateTime : Convert.ToDateTime(value);
		}

		/// <summary>
		/// Converts the object to a sbyte value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a sbyte format or returns the default value. </returns>
		public static sbyte ToSByte(this object value)
		{
			return value is sbyte castValue ? castValue : Convert.ToSByte(value);
		}

		/// <summary>
		/// Converts the object to a TimeSpan value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a TimeSpan format or returns the default value. </returns>
		public static TimeSpan ToTimeSpan(this object value)
		{
			return value is TimeSpan span ? span : TimeSpan.Zero;
		}

		/// <summary>
		/// Converts the object to a UInt16 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a UInt16 format or returns the default value. </returns>
		public static ushort ToUInt16(this object value)
		{
			return value is ushort castValue ? castValue : Convert.ToUInt16(value);
		}

		/// <summary>
		/// Converts the object to a UInt32 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a UInt32 format or returns the default value. </returns>
		public static uint ToUInt32(this object value)
		{
			return value is uint castValue ? castValue : Convert.ToUInt32(value);
		}

		/// <summary>
		/// Converts the object to a UInt64 value.
		/// </summary>
		/// <param name="value"> The value to convert. </param>
		/// <returns> The value in a UInt64 format or returns the default value. </returns>
		public static ulong ToUInt64(this object value)
		{
			return value is ulong castValue ? castValue : Convert.ToUInt64(value);
		}

		#endregion
	}
}