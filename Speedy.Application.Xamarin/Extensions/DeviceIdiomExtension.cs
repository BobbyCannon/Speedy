#region References

using System.Collections.Generic;
using Xamarin.Essentials;
using DeviceType = Speedy.Devices.DeviceType;

#endregion

namespace Speedy.Application.Xamarin.Extensions;

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

	public static DeviceIdiom ToDeviceIdiom(this DeviceType deviceType)
	{
		return _mauiLookup.ContainsKey(deviceType) ? _mauiLookup[deviceType] : DeviceIdiom.Unknown;
	}

	public static DeviceType ToDeviceType(this DeviceIdiom deviceIdiom)
	{
		return _speedyLookup.ContainsKey(deviceIdiom) ? _speedyLookup[deviceIdiom] : DeviceType.Unknown;
	}

	#endregion
}