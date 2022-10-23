#region References

using System;
using System.Globalization;
using Speedy.Protocols.Osc;
using Xamarin.Forms;

#endregion

namespace Speedy.Application.Xamarin.Converter;

/// <summary>
/// Represents a converter for to string value.
/// </summary>
public class StringConverter : IValueConverter
{
	#region Methods

	/// <inheritdoc />
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return null;
		}

		if (parameter == null)
		{
			return value.ToString();
		}

		switch (value)
		{
			case DateTime dtValue:
			{
				return dtValue.ToLocalTime().ToString(parameter.ToString());
			}

			case OscTimeTag oValue:
			{
				return oValue.ToString(parameter.ToString());
			}
		}

		return string.Format((string) parameter, value);
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	#endregion
}