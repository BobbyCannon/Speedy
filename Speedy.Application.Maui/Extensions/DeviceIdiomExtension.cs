#region References

using DeviceType = Speedy.Data.DeviceType;

#endregion

namespace Speedy.Application.Maui.Extensions;

/// <summary>
/// Device Idiom Extension
/// </summary>
public static class DeviceIdiomExtension
{
	#region Fields

	private static readonly IReadOnlyDictionary<DeviceType, DeviceIdiom> _mauiLookup;
	private static readonly IReadOnlyDictionary<DeviceIdiom, DeviceType> _speedyLookup;

	#endregion

	#region Constructors

	static DeviceIdiomExtension()
	{
		_speedyLookup = new Dictionary<DeviceIdiom, DeviceType>
		{
			{ DeviceIdiom.Desktop, DeviceType.Desktop },
			{ DeviceIdiom.Phone, DeviceType.Phone },
			{ DeviceIdiom.Tablet, DeviceType.Tablet },
			{ DeviceIdiom.TV, DeviceType.TV },
			{ DeviceIdiom.Watch, DeviceType.Watch },
			{ DeviceIdiom.Unknown, DeviceType.Unknown }
		};

		_mauiLookup = new Dictionary<DeviceType, DeviceIdiom>
		{
			{ DeviceType.Desktop, DeviceIdiom.Desktop },
			{ DeviceType.Phone, DeviceIdiom.Phone },
			{ DeviceType.Tablet, DeviceIdiom.Tablet },
			{ DeviceType.TV, DeviceIdiom.TV },
			{ DeviceType.Watch, DeviceIdiom.Watch },
			{ DeviceType.Unknown, DeviceIdiom.Unknown }
		};
	}

	#endregion

	#region Methods

	/// <summary>
	/// Convert DeviceType to a DeviceIdiom.
	/// </summary>
	/// <param name="deviceType"> The device type to convert. </param>
	/// <returns> The DeviceIdiom for the DeviceType. </returns>
	public static DeviceIdiom ToDeviceIdiom(this DeviceType deviceType)
	{
		return _mauiLookup.ContainsKey(deviceType) ? _mauiLookup[deviceType] : DeviceIdiom.Unknown;
	}

	/// <summary>
	/// Convert DeviceIdiom to a DeviceType.
	/// </summary>
	/// <param name="deviceIdiom"> The device idiom to convert. </param>
	/// <returns> The DeviceType for the DeviceIdiom. </returns>
	public static DeviceType ToDeviceType(this DeviceIdiom deviceIdiom)
	{
		return _speedyLookup.ContainsKey(deviceIdiom) ? _speedyLookup[deviceIdiom] : DeviceType.Unknown;
	}

	#endregion
}