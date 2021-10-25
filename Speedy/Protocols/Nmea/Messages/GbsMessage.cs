#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a GBS message.
	/// </summary>
	public class GbsMessage : NmeaMessage
	{
		#region Constructors

		public GbsMessage() : base(NmeaMessageType.GBS)
		{
		}

		#endregion

		#region Properties

		public double AltitudeExpectedError { get; set; }

		public string BiasEstimate { get; set; }

		public double LatitudeExpectedError { get; set; }

		public double LongitudeExpectedError { get; set; }

		public string Probability { get; set; }

		public string SatelliteId { get; set; }

		public string StandardDeviation { get; set; }

		/// <summary>
		/// Time in the hhmmss.ss format.
		/// </summary>
		public double Time { get; set; }

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
			// $GPGBS,015509.00,-0.031,-0.186,0.219,19,0.000,-0.354,6.972*4D
			//
			// .      0         1
			//	      |         |
			// $--GBS,hhmmss.ss,*hh
			//
			// 0) Time (UTC) - hhmmss.ss
			// 1) Expected error in latitude, in meters, due to bias, with noise = 0
			// 2) Expected error in longitude, in meters, due to bias, with noise = 0
			// 3) Expected error in altitude, in meters, due to bias, with noise = 0
			// 4) ID number of most likely failed satellite
			// 5) Probability of missed detection of most likely failed satellite
			// 6) Estimate of bias, in meters, on the most likely failed satellite
			// 7) Standard deviation of bias estimate
			// 8) Checksum

			StartParse(sentence);

			Time = GetArgumentAsDouble(0, 0.0);
			LatitudeExpectedError = GetArgumentAsDouble(1, 0.0);
			LongitudeExpectedError = GetArgumentAsDouble(2, 0.0);
			AltitudeExpectedError = GetArgumentAsDouble(3, 0.0);
			SatelliteId = GetArgument(4);
			Probability = GetArgument(5);
			BiasEstimate = GetArgument(6);
			StandardDeviation = GetArgument(7);
			Checksum = GetArgument(8);

			OnNmeaMessageParsed(this);
		}

		#endregion
	}
}