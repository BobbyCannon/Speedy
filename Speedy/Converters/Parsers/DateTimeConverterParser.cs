using System;
using Speedy.Extensions;

namespace Speedy.Converters.Parsers
{
	internal class DateTimeConverterParser : IStringConverterParser
	{
		#region Methods

		/// <inheritdoc />
		public bool SupportsType(Type targetType)
		{
			return targetType == typeof(DateTime);
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
				result = value.ToUtcDateTime();
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
}