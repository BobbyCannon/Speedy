#region References

using System.Runtime.CompilerServices;

#endregion

namespace Speedy.Data.Location;

/// <summary>
/// </summary>
public static class Converter
{
	#region Constructors

	static Converter()
	{
		GridLng = 60;
		Bottom = -90;
		GridLat = 60;
		Columns = 361;
		Rows = 181;
		Right = 360;
		Left = 0;
		Top = 90;
	}

	#endregion

	#region Properties

	private static double Bottom { get; }

	private static int Columns { get; }

	private static double GridLat { get; }

	private static double GridLng { get; }

	private static double Left { get; }

	private static double Right { get; }

	private static int Rows { get; }

	private static double Top { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Convert a location from one type to another. Ex. Geoid > Ellipsoid
	/// </summary>
	/// <param name="latitude"> The latitude. </param>
	/// <param name="longitude"> The longitude. </param>
	/// <param name="height"> The height. </param>
	/// <param name="flag"> The type of conversion. </param>
	/// <returns> The converted height. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double ConvertHeight(double latitude, double longitude, double height, ConvertFlag flag)
	{
		return height + ((double) flag * GetHeight(latitude, longitude));
	}

	private static double BilinearInterpolation(double[] h, double x, double y)
	{
		return (h[0] * (1.0 - x) * (1.0 - y)) + (h[1] * x * (1.0 - y)) + (h[2] * (1.0 - x) * y) + (h[3] * x * y);
	}

	private static int[] GetBoundary(ref double lat, ref double lng)
	{
		int i1, i2, j1, j2;

		var col = ((lng - Left) * 60) / GridLng;
		var row = ((lat - Bottom) * 60) / GridLat;

		i1 = (int) col;
		i2 = i1 < (Columns - 1) ? i1 + 1 : i1;
		j1 = (int) row;
		j2 = j1 < (Rows - 1) ? j1 + 1 : j1;

		lng = col - i1;
		lat = row - j1;

		return new[] { i1, i2, j1, j2 };
	}

	/// <summary>
	/// Get the geoid height
	/// </summary>
	/// <param name="lat"> latitude in degree </param>
	/// <param name="lng"> longitude in degree </param>
	/// <returns> geoid height </returns>
	private static double GetHeight(double lat, double lng)
	{
		if (lng < 0)
		{
			lng += 360.0;
		}

		if ((lng < Left) || (lng > Right) || (lat < Bottom) || (lat > Top))
		{
			return double.NaN;
		}
		// compute the position in grid model
		var position = GetBoundary(ref lat, ref lng);

		// read the grid value
		var h = GeoidGrid.ReadGrid(position);

		// bilinear interpolation
		return BilinearInterpolation(h, lng, lat);
	}

	#endregion
}

/// <summary>
/// Flags indicating conversions between heights above the geoid and heights above the ellipsoid.
/// </summary>
public enum ConvertFlag
{
	/// <summary>
	/// The multiplier for converting from heights above the geoid to heights above the ellipsoid.
	/// </summary>
	EllipsoidToGeoid = -1,

	/// <summary>
	/// No conversion.
	/// </summary>
	None = 0,

	/// <summary>
	/// The multiplier for converting from heights above the ellipsoid to heights above the geoid.
	/// </summary>
	GeoidToEllipsoid = 1
}