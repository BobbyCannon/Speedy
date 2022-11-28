#region References

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Speedy.Data.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp.Converters;

public class VisibilityConverter : IValueConverter
{
	#region Properties

	public bool Inverted { get; set; }

	#endregion

	#region Methods

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		var show = Inverted ? Visibility.Collapsed : Visibility.Visible;
		var hide = Inverted ? Visibility.Visible : Visibility.Collapsed;

		switch (value)
		{
			case bool b:
			{
				return b ? show : hide;
			}
			case DateTime d:
			{
				if ((parameter != null) && DateTime.TryParse(parameter.ToString(), out var date))
				{
					return d == date ? show : hide;
				}

				return d != default ? show : hide;
			}
			case string s:
			{
				var sBoolean = parameter != null ? string.Equals(s, parameter.ToString()) : string.IsNullOrWhiteSpace(s);
				return sBoolean ? show : hide;
			}
			case Enum e:
			{
				var eParameter = int.TryParse(parameter?.ToString(), out var ep) ? ep : 0;
				return Equals(System.Convert.ToInt32(e), eParameter) ? show : hide;
			}
			case IBasicLocation position:
			{
				return position.IsValidLocation() ? show : hide;
			}
			case byte b:
			case short s:
			case ushort us:
			case uint ui:
			case int i:
			case long l:
			case ulong ul:
			case float f:
			case double d:
			case decimal dValue:
			{
				if (decimal.TryParse(parameter?.ToString(), out var ep))
				{
					return decimal.Parse(value.ToString()) == ep ? show : hide;
				}

				return decimal.Parse(value.ToString()) == 0 ? hide : show;
			}
			default:
			{
				return value != null ? show : hide;
			}
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotSupportedException();
	}

	#endregion
}