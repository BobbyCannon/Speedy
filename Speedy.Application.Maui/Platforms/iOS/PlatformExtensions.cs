#region References

using CoreLocation;
using Foundation;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

public static class PlatformExtensions
{
	#region Methods

	public static DateTime ToDateTime(this NSDate date)
	{
		return (DateTime) date;
	}

	#endregion
}