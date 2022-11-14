#region References

using System.Globalization;

#endregion

namespace Speedy.Application.Maui.Converter;

/// <summary>
/// Represents a converter for a visibility value.
/// </summary>
public class VisibilityConverter : IValueConverter
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
		var show = Inverted ? false : true;
		var hide = Inverted ? true : false;

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
			case byte b:
			case short s:
			case ushort us:
			case int i:
			case uint ui:
			case long l:
			case ulong ul:
			case float f:
			case double d:
			case decimal dValue:
			{
				return decimal.Parse(value.ToString()) == 0 ? hide : show;
			}
			default:
			{
				return value != null ? show : hide;
			}
		}
	}

	/// <inheritdoc />
	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	#endregion
}