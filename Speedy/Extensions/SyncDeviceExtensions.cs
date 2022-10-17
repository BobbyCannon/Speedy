#region References

using System;
using System.Collections.Generic;
using Speedy.Converters;
using Speedy.Devices;
using Speedy.Sync;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for Sync Device
/// </summary>
public static class SyncDeviceExtensions
{
	#region Constants

	/// <summary>
	/// The key for the ApplicationName value for Sync Client Details.
	/// </summary>
	public const string ApplicationNameValueKey = "ApplicationName";

	/// <summary>
	/// The key for the ApplicationVersion value for Sync Client Details.
	/// </summary>
	public const string ApplicationVersionValueKey = "ApplicationVersion";

	/// <summary>
	/// The key for the DeviceId value for Sync Client Details.
	/// </summary>
	public const string DeviceIdValueKey = "DeviceId";

	/// <summary>
	/// The key for the DevicePlatform value for Sync Client Details.
	/// </summary>
	public const string DevicePlatformValueKey = "DevicePlatform";

	/// <summary>
	/// The key for the DeviceType value for Sync Client Details.
	/// </summary>
	public const string DeviceTypeValueKey = "DeviceType";

	#endregion

	#region Methods

	/// <summary>
	/// Load the sync client details into the provided sync options.
	/// </summary>
	/// <param name="device"> The device to load options into. </param>
	/// <param name="syncOptions"> The options to load. </param>
	public static void Load(this ISyncDevice device, SyncOptions syncOptions)
	{
		device.ApplicationName = TryGetValue(syncOptions.Values, ApplicationNameValueKey, string.Empty);
		device.ApplicationVersion = TryGetValue(syncOptions.Values, ApplicationVersionValueKey, new Version(0, 0, 0, 0));
		device.DeviceId = TryGetValue(syncOptions.Values, DeviceIdValueKey, string.Empty);
		device.DevicePlatform = TryGetValue(syncOptions.Values, DevicePlatformValueKey, DevicePlatform.Unknown);
		device.DeviceType = TryGetValue(syncOptions.Values, DeviceTypeValueKey, DeviceType.Unknown);

		device.Validate();
	}

	/// <summary>
	/// Load the sync client details into the provided sync options.
	/// </summary>
	/// <param name="syncOptions"> The options to load. </param>
	/// <param name="clientDetails"> The client details to load. </param>
	public static void Load(this SyncOptions syncOptions, ISyncClientDetails clientDetails)
	{
		// Add all keys that are required
		syncOptions.Values.AddIfMissing(ApplicationNameValueKey, clientDetails.ApplicationName);
		syncOptions.Values.AddIfMissing(ApplicationVersionValueKey, clientDetails.ApplicationVersion.ToString());
		syncOptions.Values.AddIfMissing(DeviceIdValueKey, clientDetails.DeviceId);
		syncOptions.Values.AddIfMissing(DevicePlatformValueKey, ((int) clientDetails.DevicePlatform).ToString());
		syncOptions.Values.AddIfMissing(DeviceTypeValueKey, ((int) clientDetails.DeviceType).ToString());
	}

	/// <summary>
	/// Validate that all the sync client details are available.
	/// </summary>
	/// <param name="device"> The device to load options into. </param>
	public static void Validate(this ISyncClientDetails device)
	{
		if (string.IsNullOrWhiteSpace(device.ApplicationName))
		{
			throw new ArgumentException($"{nameof(device.ApplicationName)} must be provided.");
		}

		if (device.ApplicationVersion.IsDefault())
		{
			throw new ArgumentException($"{nameof(device.ApplicationVersion)} must be provided.");
		}

		if (string.IsNullOrWhiteSpace(device.DeviceId))
		{
			throw new ArgumentException($"{nameof(device.DeviceId)} must be provided.");
		}

		if (device.DevicePlatform == DevicePlatform.Unknown)
		{
			throw new ArgumentException($"{nameof(device.DevicePlatform)} must be provided.");
		}

		if (device.DeviceType == DeviceType.Unknown)
		{
			throw new ArgumentException($"{nameof(device.DeviceType)} must be provided.");
		}
	}

	private static string TryGetValue(Dictionary<string, string> dictionary, string name, string defaultValue)
	{
		return dictionary.TryGetValue(name, out var value) ? value : defaultValue;
	}

	private static T TryGetValue<T>(Dictionary<string, string> dictionary, string name, T defaultValue)
	{
		try
		{
			return dictionary.TryGetValue(name, out var value)
				? StringConverter.Parse<T>(value)
				: defaultValue;
		}
		catch
		{
			return defaultValue;
		}
	}

	#endregion
}