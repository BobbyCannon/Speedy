#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Converters.Parsers;

internal class EnumConverterParser : IStringConverterParser
{
	#region Methods

	/// <inheritdoc />
	public bool SupportsType(Type targetType)
	{
		return targetType.IsEnum;
	}

	/// <inheritdoc />
	public bool TryParse(Type targetType, string value, out object result)
	{
		if (!SupportsType(targetType))
		{
			result = targetType.GetDefaultValue();
			return false;
		}

		try
		{
			result = Enum.Parse(targetType, value, true);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	#endregion
}