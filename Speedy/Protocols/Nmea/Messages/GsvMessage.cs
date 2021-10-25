#region References

using System;
using System.Collections.Generic;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a GSV message.
	/// </summary>
	public class GsvMessage : NmeaMessage
	{
		#region Constructors

		public GsvMessage() : base(NmeaMessageType.GSV)
		{
			Satellites = new List<Satellite>();
		}

		#endregion

		#region Properties

		public int NumberOfSatellitesInView { get; set; }

		public int NumberOfSentences { get; set; }

		public List<Satellite> Satellites { get; }

		public int SentenceNr { get; set; }

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
			// $GPGSV,3,1,10,01,50,304,26,03,24,245,16,08,56,204,28,10,21,059,20*77
			//
			// .      0 1 2  3  4  5   6    X
			//        | | |  |  |  |   |    |
			// $--GSV,x,u,xx,uu,vv,zzz,ss,…*hh
			//
			// 0) Total number of GSV messages - x
			// 1) Message number - u
			// 2) Satellites in view - xx
			// 3) Satellite number - uu
			// 4) Elevation in degrees - vv
			// 5) Azimuth in degrees to true - zzz
			// 6) SNR in dB - ss
			//    more satellite infos like 3-6... maximum of 4
			// X) Checksum - hh

			StartParse(sentence);

			NumberOfSentences = Convert.ToInt32(GetArgument(0));
			SentenceNr = Convert.ToInt32(GetArgument(1));
			NumberOfSatellitesInView = Convert.ToInt32(GetArgument(2));

			var satelliteCount = GetSatelliteCount(
				Convert.ToInt32(NumberOfSatellitesInView),
				Convert.ToInt32(NumberOfSentences),
				Convert.ToInt32(SentenceNr));

			for (var i = 0; i < satelliteCount; i++)
			{
				Satellites.Add(
					new Satellite
					{
						SatellitePrnNumber = GetArgument(3 + i * 4 + 0),
						ElevationDegrees = GetArgument(3 + i * 4 + 1),
						AzimuthDegrees = GetArgument(3 + i * 4 + 2),
						SignalStrength = GetArgument(3 + i * 4 + 3)
					});
			}
		}

		public override void Reset()
		{
			Satellites.Clear();
			base.Reset();
		}

		public override string ToString()
		{
			var start = string.Join(",",
				NmeaParser.GetSentenceStart(this),
				NumberOfSentences,
				SentenceNr,
				NumberOfSatellitesInView
			);

			var total = Satellites.Count;

			for (var i = 0; i < total; i++)
			{
				start += $",{Satellites[i].SatellitePrnNumber},{Satellites[i].ElevationDegrees},{Satellites[i].AzimuthDegrees},{Satellites[i].SignalStrength}";
			}

			for (var i = total; i < 4; i++)
			{
				start += ",";
			}

			return $"{start}*{Checksum}";
		}

		private int GetSatelliteCount(int numberOfSatellitesInView, int numberOfSentences, int sentenceNr)
		{
			if (numberOfSentences != sentenceNr)
			{
				return 4;
			}

			return numberOfSatellitesInView - (sentenceNr - 1) * 4;
		}

		#endregion
	}
}