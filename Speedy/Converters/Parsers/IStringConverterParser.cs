#region References

using System;

#endregion

namespace Speedy.Converters.Parsers;

/// <summary>
/// Represents a string converter parser.
/// </summary>
public interface IStringConverterParser
{
	#region Methods

	/// <summary>
	/// Determines if the target type is supported by the parser.
	/// </summary>
	/// <param name="targetType"> The target type to parse to. </param>
	/// <returns> Returns true if the type is support otherwise false. </returns>
	bool SupportsType(Type targetType);

	/// <summary>
	/// Try to convert the value to a string representation.
	/// </summary>
	/// <param name="targetType"> The type of the object to parse. </param>
	/// <param name="value"> The type value in object format. </param>
	/// <param name="result"> The string version of the object. </param>
	/// <returns> True if the convert succeeded otherwise false. </returns>
	bool TryConvertToString(Type targetType, object value, out string result);

	/// <summary>
	/// Tries to parse the provided value into the target type.
	/// </summary>
	/// <param name="targetType"> The target type to parse to. </param>
	/// <param name="value"> The value to be parsed. </param>
	/// <param name="result"> The result of the parse. </param>
	/// <returns> Returns true if the type was parsed otherwise false. </returns>
	bool TryParse(Type targetType, string value, out object result);

	#endregion
}