#pragma warning disable 1591

namespace Speedy.Protocols.Nmea
{
	public class Satellite
	{
		#region Constructors

		public Satellite()
		{
		}

		public Satellite(string id, string elevation, string azimuth, string signal)
		{
			SatellitePrnNumber = id;
			ElevationDegrees = elevation;
			AzimuthDegrees = azimuth;
			SignalStrength = signal;
		}

		#endregion

		#region Properties

		public string AzimuthDegrees { get; set; }

		public string ElevationDegrees { get; set; }

		public string SatellitePrnNumber { get; set; }

		public string SignalStrength { get; set; }

		#endregion
	}
}