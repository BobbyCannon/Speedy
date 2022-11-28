#region References

using System;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

#endregion

namespace Speedy.Application.Uwp.Converters;

public class BrushConverter : IValueConverter
{
	#region Methods

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		switch (value)
		{
			case bool bValue:
			{
				var pValue = (parameter?.ToString() ?? "").Split(",");
				return pValue.Length == 2
					? new SolidColorBrush(bValue
						? ToColor(pValue[0], "SystemBaseHighColor")
						: ToColor(pValue[1], "SystemBaseMediumLowColor"))
					: new SolidColorBrush(bValue
						? ToColor("", "SystemBaseHighColor")
						: ToColor("", "SystemBaseMediumLowColor"));
			}
			case Color color:
			{
				return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
			}
			case System.Drawing.Color color:
			{
				return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
			}
			default:
			{
				return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
			}
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}

	private Color ToColor(string value, string defaultResourceValue)
	{
		if ((value == null) || ((value.Length != 7) && (value.Length != 9)))
		{
			return (Color) Windows.UI.Xaml.Application.Current.Resources[defaultResourceValue];
		}

		if (!int.TryParse(value.Substring(1, 2), NumberStyles.AllowHexSpecifier, null, out var r)
			|| !int.TryParse(value.Substring(3, 2), NumberStyles.AllowHexSpecifier, null, out var g)
			|| !int.TryParse(value.Substring(5, 2), NumberStyles.AllowHexSpecifier, null, out var b))
		{
			return (Color) Windows.UI.Xaml.Application.Current.Resources[defaultResourceValue];
		}

		var a = value.Length != 9 ? 255 : int.TryParse(value.Substring(7, 2), NumberStyles.AllowHexSpecifier, null, out var p) ? p : 255;

		return Color.FromArgb((byte) a, (byte) r, (byte) g, (byte) b);
	}

	#endregion
}