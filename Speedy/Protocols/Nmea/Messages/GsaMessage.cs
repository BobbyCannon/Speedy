#region References

using System;
using System.Collections.Generic;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// Represents a GSA message.
/// </summary>
public class GsaMessage : NmeaMessage
{
	#region Constructors

	public GsaMessage() : base(NmeaMessageType.GSA)
	{
		PrnsOfSatellitesUsedForFix = new List<int>();
	}

	#endregion

	#region Properties

	public string AutoSelection { get; set; }

	public string Fix3D { get; set; }

	public double HorizontalDilutionOfPrecision { get; set; }

	public double PositionDilutionOfPrecision { get; set; }

	/// <summary>
	/// Pseudo-Random Noise for satellites used for fix.
	/// </summary>
	public List<int> PrnsOfSatellitesUsedForFix { get; }

	public double VerticalDilutionOfPrecision { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Add pseudo-random noise for a satellite.
	/// </summary>
	/// <param name="prn"> </param>
	public void AddPrn(string prn)
	{
		if (!string.IsNullOrEmpty(prn))
		{
			PrnsOfSatellitesUsedForFix.Add(Convert.ToInt32(prn));
		}
	}

	public override void Parse(string sentence)
	{
		// $GNGSA,A,3,01,18,32,08,11,,,,,,,,6.16,1.86,5.88*16
		//
		//.       0 1 2                           14  15  16  17
		//	      | | |                           |   |   |   |
		// $--GSA,a,a,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x.x,x.x,x.x*hh
		//
		// 0) Selection mode
		//    M - Manual, forced to operate in 2D or 3D mode
		//    A - Automatic, allowed to automatically switch 2D/3D
		// 1) Mode
		//    1 - Fix not available
		//    2 - 2D
		//    3 - 3D
		// 2) ID of 1st satellite used for fix
		//    GPS and Beidou satellites are differentiated by the GP and BD prefix. Maximally 12 satellites are included in each GSA sentence
		//    01 ~ 32 are for GPS;
		//    33 ~ 64 are for SBAS (PRN minus 87);
		//    65 ~ 96 are for GLONASS (64 plus slot numbers);
		//    193 ~ 197 are for QZSS;
		//    01 ~ 37 are for Beidou (BD PRN).
		// 3) ID of 2nd satellite used for fix
		// ...
		// 13) ID of 12th satellite used for fix
		// 14) PDOP in meters
		// 15) HDOP in meters
		// 16) VDOP in meters
		// 17) Checksum

		StartParse(sentence);

		AutoSelection = GetArgument(0);
		Fix3D = GetArgument(1);

		AddPrn(GetArgument(2));
		AddPrn(GetArgument(3));
		AddPrn(GetArgument(4));
		AddPrn(GetArgument(5));
		AddPrn(GetArgument(6));
		AddPrn(GetArgument(7));
		AddPrn(GetArgument(8));
		AddPrn(GetArgument(9));
		AddPrn(GetArgument(10));
		AddPrn(GetArgument(11));
		AddPrn(GetArgument(12));
		AddPrn(GetArgument(13));

		PositionDilutionOfPrecision = Convert.ToDouble(GetArgument(14, "0"));
		HorizontalDilutionOfPrecision = Convert.ToDouble(GetArgument(15, "0"));
		VerticalDilutionOfPrecision = Convert.ToDouble(GetArgument(16, "0"));
	}

	public override void Reset()
	{
		PrnsOfSatellitesUsedForFix.Clear();
		base.Reset();
	}

	public override string ToString()
	{
		var start = string.Join(",",
			NmeaParser.GetSentenceStart(this),
			AutoSelection,
			Fix3D
		);

		var totalPrn = PrnsOfSatellitesUsedForFix.Count;

		for (var i = 0; i < totalPrn; i++)
		{
			start += $",{PrnsOfSatellitesUsedForFix[i]:00}";
		}

		for (var i = totalPrn; i < 12; i++)
		{
			start += ",";
		}

		start += ",";
		start += PositionDilutionOfPrecision.ToString("##0.0#") + ",";
		start += HorizontalDilutionOfPrecision.ToString("##0.0#") + ",";
		start += VerticalDilutionOfPrecision.ToString("##0.0#");

		return $"{start}*{Checksum}";
	}

	#endregion
}