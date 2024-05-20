#region References

using System.Linq;
using System.Net.NetworkInformation;
using Speedy.Runtime;

/* Unmerged change from project 'Cornerstone (netstandard2.0)'
Before:
using Cornerstone.Runtime;
After:
using Cornerstone;
using Cornerstone.Internal;
using Cornerstone.Internal;
using Cornerstone.Runtime;
*/

#endregion

namespace Speedy.Internal;

/// <summary>
/// An implementation of <see cref="IDeviceIdComponent" /> that uses the MAC Address of the PC.
/// </summary>
internal class MacAddressDeviceIdComponent : IDeviceIdComponent
{
	#region Fields

	/// <summary>
	/// A value determining whether wireless devices should be excluded.
	/// </summary>
	private readonly bool _excludeWireless;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="MacAddressDeviceIdComponent" /> class.
	/// </summary>
	/// <param name="excludeWireless"> A value determining whether wireless devices should be excluded. </param>
	public MacAddressDeviceIdComponent(bool excludeWireless)
	{
		_excludeWireless = excludeWireless;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the component value.
	/// </summary>
	/// <returns> The component value. </returns>
	public string GetValue()
	{
		var values = NetworkInterface.GetAllNetworkInterfaces()
			.Where(x => !_excludeWireless || (x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
			.Select(x => x.GetPhysicalAddress().ToString())
			.Where(x => x != "000000000000")
			.Select(MacAddressFormatter.FormatMacAddress)
			.ToList();

		values.Sort();

		return values.Count > 0
			? string.Join(",", values.ToArray())
			: null;
	}

	#endregion
}