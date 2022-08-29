#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

#endregion

namespace Speedy.Storage
{
	/// <inheritdocs />
	public class SyncKeyComparer : IComparer<string>
	{
		#region Fields

		private readonly CultureInfo _cultureInfo;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the natural comparer.
		/// </summary>
		public SyncKeyComparer() : this(CultureInfo.CurrentCulture)
		{
		}

		/// <summary>
		/// Instantiates an instance of the natural comparer.
		/// </summary>
		/// <param name="cultureInfo"> </param>
		public SyncKeyComparer(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}

		#endregion

		#region Methods

		/// <inheritdocs />
		public int Compare(string x, string y)
		{
			// simple cases
			if (x == y) // also handles null
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return +1;
			}

			var ix = 0;
			var iy = 0;

			while ((ix < x.Length) && (iy < y.Length))
			{
				if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
				{
					// We found numbers, so grab both numbers
					var ix1 = ix++;
					var iy1 = iy++;

					while ((ix < x.Length) && char.IsDigit(x[ix]))
					{
						ix++;
					}
					while ((iy < y.Length) && char.IsDigit(y[iy]))
					{
						iy++;
					}

					var numberFromX = x.Substring(ix1, ix - ix1);
					var numberFromY = y.Substring(iy1, iy - iy1);

					// Pad them with 0's to have the same length
					var maxLength = Math.Max(numberFromX.Length, numberFromY.Length);
					numberFromX = numberFromX.PadLeft(maxLength, '0');
					numberFromY = numberFromY.PadLeft(maxLength, '0');

					var comparison = _cultureInfo.CompareInfo.Compare(numberFromX, numberFromY);
					if (comparison != 0)
					{
						return comparison;
					}
				}
				else
				{
					var comparison = _cultureInfo.CompareInfo.Compare(x, ix, 1, y, iy, 1);
					if (comparison != 0)
					{
						return comparison;
					}
					ix++;
					iy++;
				}
			}

			// we should not be here with no parts left, they're equal
			Debug.Assert((ix < x.Length) || (iy < y.Length));

			// we still got parts of x left, y comes first
			if (ix < x.Length)
			{
				return +1;
			}

			// we still got parts of y left, x comes first
			return -1;
		}

		#endregion
	}
}