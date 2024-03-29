﻿#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Converters.Parsers;

internal class UriStringConverterParser : IStringConverterParser
{
	#region Methods

	/// <inheritdoc />
	public bool SupportsType(Type targetType)
	{
		return targetType == typeof(Uri);
	}

	public bool TryConvertToString(Type targetType, object value, out string result)
	{
		if (value is Uri uri)
		{
			result = uri.ToString();
			return true;
		}

		result = default;
		return false;
	}

	/// <inheritdoc />
	public bool TryParse(Type targetType, string value, out object result)
	{
		if (!SupportsType(targetType))
		{
			result = targetType.GetDefaultValue();
			return false;
		}

		var response = Uri.TryCreate(value, UriKind.Absolute, out var pResult);
		result = response ? pResult : targetType.GetDefaultValue();
		return response;
	}

	#endregion
}