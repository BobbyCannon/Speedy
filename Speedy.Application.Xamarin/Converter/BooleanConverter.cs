#region References

using System;
using System.Globalization;
using Xamarin.Forms;

#endregion

namespace Speedy.Application.Xamarin.Converter;

/// <summary>
/// Represents a converter for a boolean value.
/// </summary>
public class BooleanConverter : IValueConverter
{
	#region Properties

	/// <summary>
	/// Flag to determine if this converter is inverting the logic.
	/// </summary>
	public bool Inverted { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var show = false;

		switch (value)
		{
			case bool b:
			{
				show = parameter is bool bParameter ? bParameter == b : b;
				break;
			}
			case string s:
			{
				show = !string.IsNullOrWhiteSpace(s);
				break;
			}
			case int i:
			{
				show = i != 0;
				break;
			}
		}

		return Inverted ? !show : show;
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var response = (bool) value;

		if (Inverted)
		{
			response = !response;
		}

		if (targetType.FullName == typeof(bool).FullName)
		{
			return response;
		}

		throw new NotSupportedException();
	}

	#endregion
}