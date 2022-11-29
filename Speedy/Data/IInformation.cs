#region References

using System;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents information returned by an <see cref="IInformationProvider" />.
/// </summary>
public interface IInformation : IBindable
{
	#region Properties

	/// <summary>
	/// Determines if the device information has a value.
	/// </summary>
	/// <remarks>
	/// Each device information could / will have different value members.
	/// Ex. VerticalLocation will have Altitude, where HorizontalLocation will have Latitude, Longitude.
	/// </remarks>
	bool HasValue { get; }

	/// <summary>
	/// Represents a global unique ID to identify an information type.
	/// </summary>
	Guid InformationId { get; }

	/// <summary>
	/// The name of the provider that is the source of this information.
	/// </summary>
	string ProviderName { get; set; }

	/// <summary>
	/// The name of the source of the information. Ex. Hardware, Software, Simulated, Wifi, GPS, etc
	/// </summary>
	string SourceName { get; set; }

	/// <summary>
	/// The original time of the information was captured.
	/// </summary>
	DateTime StatusTime { get; set; }

	#endregion
}