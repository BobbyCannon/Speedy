using System;
using Speedy.Exceptions;

namespace Speedy.Devices.Location;

/// <summary>
/// Location exception
/// </summary>
public class GeolocationException  : SpeedyException
{
	#region Constructors

	/// <summary>
	/// Location exception
	/// </summary>
	/// <param name="error"> </param>
	public GeolocationException(GeolocationError error)
		: base("A geolocation error occurred: " + error)
	{
		if (!Enum.IsDefined(typeof(GeolocationError), error))
		{
			throw new ArgumentException("error is not a valid Geolocation Error member", nameof(error));
		}

		Error = error;
	}

	/// <summary>
	/// Geolocation error
	/// </summary>
	/// <param name="error"> </param>
	/// <param name="innerException"> </param>
	public GeolocationException(GeolocationError error, Exception innerException)
		: base("A geolocation error occurred: " + error, innerException)
	{
		if (!Enum.IsDefined(typeof(GeolocationError), error))
		{
			throw new ArgumentException("error is not a valid Geolocation Error member", nameof(error));
		}

		Error = error;
	}

	#endregion

	#region Properties

	public GeolocationError Error { get; }

	#endregion
}