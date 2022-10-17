#region References

using System;
using System.Collections.Generic;
using Speedy.Converters.Parsers;
using Speedy.Extensions;

#endregion

namespace Speedy.Converters;

/// <summary>
/// Represents a string version of a typed value.
/// </summary>
public abstract class StringConverter
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the string converter for the type.
	/// </summary>
	/// <param name="targetString"> The target in string format. </param>
	/// <param name="targetType"> The type of the target result. </param>
	/// <exception cref="ArgumentOutOfRangeException">
	/// $"Unable to create type '{typeof(T).Name}' from value '{value}'."
	/// </exception>
	protected StringConverter(string targetString, Type targetType)
	{
		TargetType = targetType;
		TargetString = targetString;
	}

	static StringConverter()
	{
		Parsers = new List<IStringConverterParser>
		{
			new DateTimeConverterParser(),
			new IsoDateTimeConverterParser(),
			new EnumConverterParser(),
			new StringConverterParser(),
			new ReflectionStringConverterParser(),
			new UriStringConverterParser()
		};
	}

	#endregion

	#region Properties

	/// <summary>
	/// The parsers for the String Converter to use for parsing.
	/// </summary>
	public static List<IStringConverterParser> Parsers { get; set; }

	/// <summary>
	/// The target in a string format.
	/// </summary>
	public string TargetString { get; private set; }

	/// <summary>
	/// The target in the typed format.
	/// </summary>
	public Type TargetType { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Creates an instance of the StringConverter.
	/// </summary>
	/// <param name="targetType"> The target type. </param>
	/// <param name="value"> </param>
	/// <returns> </returns>
	public static StringConverter Create(Type targetType, string value)
	{
		return (StringConverter) typeof(StringConverter<>).CreateInstance(new[] { targetType }, value);
	}

	/// <summary>
	/// Parse the value from the initially constructed value.
	/// </summary>
	/// <returns> The object version of the string. </returns>
	public object Parse()
	{
		return TryParse(TargetType, TargetString, out var result)
			? result
			: TargetType.GetDefaultValue();
	}

	/// <summary>
	/// Parse the value from the provided value.
	/// </summary>
	/// <param name="value"> The type value in string format. </param>
	/// <returns> The object version of the string. </returns>
	public object Parse(string value)
	{
		TargetString = value;
		return TryParse(TargetType, value, out var result)
			? result
			: TargetType.GetDefaultValue();
	}

	/// <summary>
	/// Parse the value from the provided value.
	/// </summary>
	/// <param name="value"> The type value in string format. </param>
	/// <returns> The object version of the string. </returns>
	public static T Parse<T>(string value)
	{
		return (T) Parse(typeof(T), value);
	}

	/// <summary>
	/// Parse the value from the provided value.
	/// </summary>
	/// <param name="targetType"> The type of the object to parse. </param>
	/// <param name="value"> The type value in string format. </param>
	/// <returns> The object version of the string. </returns>
	public static object Parse(Type targetType, string value)
	{
		return TryParse(targetType, value, out var result)
			? result
			: targetType.GetDefaultValue();
	}

	/// <summary>
	/// Try to parse the value from the provided value.
	/// </summary>
	/// <param name="value"> The type value in string format. </param>
	/// <param name="result"> The object version of the string. </param>
	/// <returns> True if the parse succeeded otherwise false. </returns>
	public bool TryParse(string value, out object result)
	{
		TargetString = value;
		return TryParse(TargetType, value, out result);
	}

	/// <summary>
	/// Try to parse the value from the provided value.
	/// </summary>
	/// <param name="value"> The type value in string format. </param>
	/// <param name="result"> The object version of the string. </param>
	/// <returns> True if the parse succeeded otherwise false. </returns>
	public static bool TryParse<T>(string value, out T result)
	{
		if (TryParse(typeof(T), value, out var response))
		{
			result = (T) response;
			return true;
		}

		result = default;
		return false;
	}

	/// <summary>
	/// Try to parse the value from the provided value.
	/// </summary>
	/// <param name="targetType"> The type of the object to parse. </param>
	/// <param name="value"> The type value in string format. </param>
	/// <param name="result"> The object version of the string. </param>
	/// <returns> True if the parse succeeded otherwise false. </returns>
	public static bool TryParse(Type targetType, string value, out object result)
	{
		if (targetType.IsNullable() && (value == null))
		{
			result = null;
			return true;
		}

		foreach (var parser in Parsers)
		{
			if (!parser.SupportsType(targetType))
			{
				continue;
			}

			return parser.TryParse(targetType, value, out result);
		}

		result = targetType.GetDefaultValue();
		return false;
	}

	#endregion
}

/// <summary>
/// Represents a string version of a typed value.
/// </summary>
public class StringConverter<T> : StringConverter
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the stringed type.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// $"Unable to create type '{typeof(T).Name}' from value '{value}'."
	/// </exception>
	public StringConverter(string value) : base(value, typeof(T))
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Cast the string value to the string converter for the type.
	/// </summary>
	/// <param name="value"> The type value in string format. </param>
	/// <returns> The string converter for the type provided. </returns>
	public static implicit operator StringConverter<T>(string value)
	{
		return new StringConverter<T>(value);
	}

	/// <summary>
	/// Cast the typed value to the string converter.
	/// </summary>
	/// <param name="value"> The value in the typed format. </param>
	/// <returns> The string converter for the type provided. </returns>
	public static implicit operator StringConverter<T>(T value)
	{
		return new StringConverter<T>(value.ToString());
	}

	/// <summary>
	/// Cast the string converter to the target type.
	/// </summary>
	/// <param name="value"> The string converter for the provided type. </param>
	/// <returns> The value in the typed format. </returns>
	public static implicit operator T(StringConverter<T> value)
	{
		return (T) value.Parse();
	}

	/// <summary>
	/// Cast the string converter to the target string value.
	/// </summary>
	/// <param name="value"> The string converter for the provided string value. </param>
	/// <returns> The string converter in target string format. </returns>
	public static implicit operator string(StringConverter<T> value)
	{
		return value.TargetString;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return TargetString;
	}

	#endregion
}