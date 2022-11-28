#region References

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

#endregion

namespace Speedy.Application.Uwp.Converters;

public class HeadingConverter : DependencyObject, IValueConverter
{
	#region Fields

	public static readonly DependencyProperty OffsetDependencyProperty = DependencyProperty.Register(nameof(Offset), typeof(double), typeof(object), new PropertyMetadata(0));

	#endregion

	#region Properties

	public double Offset
	{
		get => (double) GetValue(OffsetDependencyProperty);
		set => SetValue(OffsetDependencyProperty, value);
	}

	#endregion

	#region Methods

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var hValue = value is double dValue ? dValue : 0;

		if (value is float fValue)
		{
			hValue = fValue;
		}

		var hOffset = Offset;
		var response = hValue - hOffset;

		if (response > 360)
		{
			response = response % 360;
		}

		if (response < 0)
		{
			response = 360 + response;
		}

		return response;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}

	#endregion
}