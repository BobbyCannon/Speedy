#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea
{
	/// <summary>
	/// Position - location
	/// </summary>
	public class NmeaLocation
	{
		#region Constructors

		public NmeaLocation(string degree, string indicator)
		{
			Degree = degree;
			Indicator = indicator;
		}

		#endregion

		#region Properties

		public string Degree { get; }

		public string Indicator { get; }

		#endregion

		#region Methods

		/// <summary>
		/// XXYY.YYYY = XX + (YYYYYY / 600000) graden.
		/// (d)dd + (mm.mmmm/60) (* -1 for W and S)
		/// </summary>
		/// <returns> </returns>
		public double ToDecimal()
		{
			if (string.IsNullOrEmpty(Degree) || string.IsNullOrEmpty(Indicator))
			{
				return -1;
			}

			// ddmm.mmmm
			var ddmm = Degree.Split('.');
			var dd = ddmm[0].Substring(0, ddmm[0].Length - 2);
			var mm = ddmm[0].Substring(ddmm[0].Length - 2);
			var mmmm = ddmm[1];
			var minute = mm + "." + mmmm;

			// indicators
			var nesw = Indicator;
			var plusMinus = (nesw == "S") || (nesw == "W") ? -1 : 1;

			var result = (Convert.ToDouble(dd) + (Convert.ToDouble(minute) / 60.0)) * plusMinus;
			return result;
		}

		public override string ToString()
		{
			return ToDecimal().ToString("N8");
		}

		#endregion
	}
}