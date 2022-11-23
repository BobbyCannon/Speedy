#region References

using System;

#endregion

namespace Speedy.Data.Location;

/// <summary>
/// Represents the flag state for a provider location.
/// </summary>
[Flags]
public enum LocationFlags
{
	/// <summary>
	/// No flags
	/// </summary>
	None = 0,

	/// <summary>
	/// Location has heading.
	/// </summary>
	HasHeading = 0b1,

	/// <summary>
	/// Location has speed.
	/// </summary>
	HasSpeed = 0b10,

	/// <summary>
	/// Location has primary value (ex. alt or lat/long)
	/// </summary>
	HasLocation = 0b100,

	/// <summary>
	/// All flags
	/// </summary>
	All = 0b111
}