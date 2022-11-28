#region References

using System;
using Windows.UI.Xaml.Data;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.Application.Uwp.Converters;

public class StringConverter : IValueConverter
{
	#region Methods

	public object Convert(object value, Type targetType, object parameter, string language)
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

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}

	#endregion
}