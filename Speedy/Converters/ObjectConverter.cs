#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using Speedy.Extensions;
using Speedy.Protocols.Osc;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy.Converters;

/// <summary>
/// Converts an object to different types
/// </summary>
public static class ObjectConverter
{
	#region Constructors

	static ObjectConverter()
	{
		AllNumericTypes = new List<Type>
			{
				typeof(byte),
				typeof(sbyte),
				typeof(char),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
				typeof(nint),
				typeof(nuint)
			}
			.AsReadOnly();

		var allTypes = new List<Type>
		{
			typeof(bool),
			typeof(char),
			typeof(string),
			typeof(IntPtr),
			typeof(UIntPtr),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(OscTimeTag),
			typeof(Guid),
			typeof(ShortGuid)
		};

		allTypes.AddRange(AllNumericTypes);
		AllTypes = allTypes.AsReadOnly();
	}

	#endregion

	#region Properties

	/// <summary>
	/// All numeric types that are supported (known) by the object converter.
	/// </summary>
	public static IReadOnlyList<Type> AllNumericTypes { get; }

	/// <summary>
	/// All types that are supported (known) by the object converter.
	/// </summary>
	public static IReadOnlyList<Type> AllTypes { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Convert the value to the provided type.
	/// </summary>
	/// <typeparam name="T"> The expected / desired type. </typeparam>
	/// <param name="value"> The value instance. </param>
	/// <returns> The value in the type provider </returns>
	/// <exception cref="NotSupportedException"> The type ({type.FullName}) could not be converted. </exception>
	public static T Convert<T>(object value)
	{
		return TryConvert(typeof(T), value, out var result)
			? (T) result
			: throw new FormatException("The value format is not supported.");
	}

	/// <summary>
	/// Convert the value to the provided type.
	/// </summary>
	/// <param name="type"> The expected / desired type. </param>
	/// <param name="value"> The value instance. </param>
	/// <returns> The value in the type provider </returns>
	/// <exception cref="NotSupportedException"> The type ({type.FullName}) could not be converted. </exception>
	public static object Convert(Type type, object value)
	{
		return TryConvert(type, value, out var result)
			? result
			: throw new FormatException("The value format is not supported.");
	}

	/// <summary>
	/// Converts the object to a byte value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a byte format or returns the default value. </returns>
	public static byte ToByte(this object value)
	{
		return value is byte castValue ? castValue : System.Convert.ToByte(value);
	}

	/// <summary>
	/// Converts the object to a char value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a char format or returns the default value. </returns>
	public static char ToChar(this object value)
	{
		return value is char castValue ? castValue : System.Convert.ToChar(value);
	}

	/// <summary>
	/// Converts the object to a DateTime value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a DateTime format or returns the default value. </returns>
	public static DateTime ToDateTime(this object value)
	{
		return value is DateTime dateTime ? dateTime : System.Convert.ToDateTime(value);
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
		return value is decimal castValue ? castValue : System.Convert.ToDecimal(value);
	}

	/// <summary>
	/// Converts the object to a double value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a double format or returns the default value. </returns>
	public static double ToDouble(this object value)
	{
		return value is double castValue ? castValue : System.Convert.ToDouble(value);
	}

	/// <summary>
	/// Converts the object to a float value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a float format or returns the default value. </returns>
	public static float ToFloat(this object value)
	{
		return value is float castValue ? castValue : System.Convert.ToSingle(value);
	}

	/// <summary>
	/// Converts the object to a Int16 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a Int16 format or returns the default value. </returns>
	public static short ToInt16(this object value)
	{
		return value is short castValue ? castValue : System.Convert.ToInt16(value);
	}

	/// <summary>
	/// Converts the object to a Int32 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a Int32 format or returns the default value. </returns>
	public static int ToInt32(this object value)
	{
		return value is int castValue ? castValue : System.Convert.ToInt32(value);
	}

	/// <summary>
	/// Converts the object to a Int64 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a Int64 format or returns the default value. </returns>
	public static long ToInt64(this object value)
	{
		return value is long castValue ? castValue : System.Convert.ToInt64(value);
	}

	/// <summary>
	/// Converts the object to a DateTime value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a DateTime format or returns the default value. </returns>
	public static OscTimeTag ToOscTimeTag(this object value)
	{
		return value is OscTimeTag dateTime ? dateTime : System.Convert.ToDateTime(value);
	}

	/// <summary>
	/// Converts the object to a sbyte value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a sbyte format or returns the default value. </returns>
	public static sbyte ToSByte(this object value)
	{
		return value is sbyte castValue ? castValue : System.Convert.ToSByte(value);
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
		return value is ushort castValue ? castValue : System.Convert.ToUInt16(value);
	}

	/// <summary>
	/// Converts the object to a UInt32 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a UInt32 format or returns the default value. </returns>
	public static uint ToUInt32(this object value)
	{
		return value is uint castValue ? castValue : System.Convert.ToUInt32(value);
	}

	/// <summary>
	/// Converts the object to a UInt64 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <returns> The value in a UInt64 format or returns the default value. </returns>
	public static ulong ToUInt64(this object value)
	{
		return value is ulong castValue ? castValue : System.Convert.ToUInt64(value);
	}

	/// <summary>
	/// Convert the value to the provided type.
	/// </summary>
	/// <param name="type"> The expected / desired type. </param>
	/// <param name="value"> The value instance. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	/// <exception cref="NotSupportedException"> The type ({type.FullName}) could not be converted. </exception>
	public static bool TryConvert(Type type, object value, out object result)
	{
		if (value == null)
		{
			result = type.IsValueType ? type.GetDefaultValue() : null;
			return true;
		}

		var vType = value.GetType();
		if (vType == type)
		{
			result = value;
			return true;
		}

		if (type.IsEnum)
		{
			return TryToEnum(type, value, out result);
		}

		if (value is JToken jValue)
		{
			return TryGetValue(jValue, type, out result);
		}
		if (value is string sValue)
		{
			return StringConverter.TryParse(type, sValue, out result);
		}
		if (type == typeof(object))
		{
			// Change requesting type to the desired type provided.
			type = vType;
		}
		else if (type == typeof(byte))
		{
			if (TryConvertToByte(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(char))
		{
			if (TryConvertToChar(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(DateTime))
		{
			if (TryConvertToDateTime(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(DateTimeOffset))
		{
			if (TryConvertToDateTimeOffset(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if ((type == typeof(Guid)) || (type == typeof(Guid?)))
		{
			if (TryConvertToGuid(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(long))
		{
			if (TryConvertToInt64(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(ulong))
		{
			if (TryConvertToUInt64(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(sbyte))
		{
			if (TryConvertToSByte(value, out var converted))
			{
				result = converted;
				return true;
			}

			result = null;
			return false;
		}
		else if (type == typeof(string))
		{
			if (StringConverter.TryConvertToString(value, out var sResult))
			{
				result = sResult;
				return true;
			}

			result = null;
			return false;
		}

		try
		{
			result = System.Convert.ChangeType(value, type);
			return true;
		}
		catch
		{
			result = null;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a byte value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToByte(this object value, out byte result)
	{
		try
		{
			result = value is byte castValue ? castValue : System.Convert.ToByte(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a char value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToChar(this object value, out char result)
	{
		try
		{
			result = value is char castValue ? castValue : System.Convert.ToChar(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a DateTime value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToDateTime(this object value, out DateTime result)
	{
		try
		{
			result = value is DateTime dateTime ? dateTime : System.Convert.ToDateTime(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a DateTimeOffset value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToDateTimeOffset(this object value, out DateTimeOffset result)
	{
		try
		{
			result = value is DateTimeOffset offsetValue
				? offsetValue
				: new DateTimeOffsetConverter().ConvertFrom(value) is DateTimeOffset offset
					? offset
					: default;
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a Guid value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToGuid(this object value, out Guid result)
	{
		try
		{
			if (value is Guid castValue)
			{
				result = castValue;
				return true;
			}

			if (Guid.TryParse(value.ToString(), out result))
			{
				return true;
			}

			result = default;
			return false;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a Int64 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToInt64(this object value, out long result)
	{
		try
		{
			result = value is long castValue ? castValue : System.Convert.ToInt64(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a sbyte value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToSByte(this object value, out sbyte result)
	{
		try
		{
			result = value is sbyte castValue ? castValue : System.Convert.ToSByte(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to a UInt64 value.
	/// </summary>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryConvertToUInt64(this object value, out ulong result)
	{
		try
		{
			result = value is ulong castValue ? castValue : System.Convert.ToUInt64(value);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Converts the object to the enum type.
	/// </summary>
	/// <param name="type"> The type of the enum. </param>
	/// <param name="value"> The value to convert. </param>
	/// <param name="result"> The result if the convert worked. </param>
	/// <returns> Try if the convert was successful otherwise false. </returns>
	public static bool TryToEnum(Type type, object value, out object result)
	{
		return StringConverter.TryParse(type, value.ToString() ?? "0", out result);
	}

	internal static bool TryGetObject(JObject jObject, Type type, out object value)
	{
		var directProperties = type.GetCachedProperties();
		var response = type == typeof(object)
			? new Dictionary<string, object>()
			: Activator.CreateInstance(type);

		var responseDictionary = response as Dictionary<string, object>;

		foreach (var jValue in jObject)
		{
			var p = directProperties.FirstOrDefault(x => string.Equals(x.Name, jValue.Key, StringComparison.OrdinalIgnoreCase));
			if (p == null)
			{
				responseDictionary?.AddOrUpdate(jValue.Key,
					jValue.Value != null
					&& TryGetValue(jValue.Value, PartialUpdateConverter.ConvertType(jValue.Value.Type), out var jValueValue)
						? jValueValue
						: jValue.Value
				);
				continue;
			}

			if (TryGetValue(jValue.Value, p.PropertyType, out var pValue))
			{
				p.SetValue(response, pValue);
			}
		}

		value = response;
		return true;
	}

	internal static bool TryGetValue(JToken token, Type requestedType, out object value)
	{
		// Property of array must be IEnumerable (ignoring some types like string)
		if (token is JArray jArray && requestedType.IsEnumerable())
		{
			if (requestedType.IsArray || (requestedType == typeof(Array)))
			{
				var array = Array.CreateInstance(requestedType.GetElementType() ?? typeof(object), jArray.Count);

				for (var index = 0; index < jArray.Count; index++)
				{
					var jArrayValue = jArray[index];
					if (TryGetValue(jArrayValue, requestedType.GetElementType() ?? typeof(object), out var arrayValue))
					{
						array.SetValue(arrayValue, index);
					}
				}

				value = array;
				return true;
			}

			var genericType = requestedType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
			var genericList = (IList) requestedType.CreateInstanceOfGeneric();

			foreach (var jArrayValue in jArray)
			{
				if (TryGetValue(jArrayValue, genericType, out var arrayValue))
				{
					genericList?.Add(arrayValue);
				}
			}

			value = genericList;
			return true;
		}

		if (token is JObject jObject)
		{
			return TryGetObject(jObject, requestedType, out value);
		}

		if (token is not JValue jValue)
		{
			value = null;
			return false;
		}

		if ((jValue.Type == JTokenType.Null) && (jValue.Value == null))
		{
			value = null;
			return true;
		}

		try
		{
			// todo: in the future, just use Convert. However it still needs work before we can use it
			value = System.Convert.ChangeType(jValue, requestedType);
			return true;
		}
		catch
		{
			value = null;
			return false;
		}
	}

	#endregion
}