#region References

using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Data;
using Speedy.Application.Uwp.Extensions;
using Speedy.Data.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp.Converters;

public class GeoPointConverter : IValueConverter
{
	#region Methods

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		switch (value)
		{
			case Location location:
			{
				var basic = (BasicLocation) location;
				return basic.ToGeopoint();
			}
			case IBasicLocation location:
			{
				// All basic locations are going to be terrain because we cannot guarantee the altitude is valid.
				var position = GetBasicGeoposition(location);
				position.Altitude = 0;
				var reference = AltitudeReferenceSystem.Terrain;
				return new Geopoint(position, reference);
			}
			default:
			{
				var position = new BasicGeoposition { Altitude = 0 };
				return new Geopoint(position, AltitudeReferenceSystem.Terrain);
			}
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}

	private BasicGeoposition GetBasicGeoposition(IBasicLocation state)
	{
		return !state.IsValidLocation()
			? new BasicGeoposition { Altitude = 0 }
			: state.ToBasicGeoposition();
	}

	#endregion
}