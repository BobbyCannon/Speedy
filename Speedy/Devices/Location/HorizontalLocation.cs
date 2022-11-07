#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location.
/// </summary>
public class HorizontalLocation : Bindable, IHorizontalLocation
{
	#region Properties

	/// <inheritdoc />
	public bool HasHorizontalAccuracy => this.HasSupportedHorizontalAccuracy();

	/// <inheritdoc />
	public double HorizontalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType HorizontalAccuracyReference { get; set; }

	/// <inheritdoc />
	public string HorizontalSourceName { get; set; }

	/// <inheritdoc />
	public DateTime HorizontalStatusTime { get; set; }

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	#endregion
}