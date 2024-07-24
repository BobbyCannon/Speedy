using System;
using System.Globalization;
using System.Windows.Data;

namespace Speedy.Example.Wpf;

public class InvertValueConverter : IValueConverter
{
	#region Methods

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value switch
		{
			bool sValue => !sValue,
			_ => value
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	#endregion
}