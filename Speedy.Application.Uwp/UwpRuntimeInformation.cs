#region References

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Microsoft.Toolkit.Uwp.Helpers;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp;

/// <inheritdoc />
public class UwpRuntimeInformation : RuntimeInformation
{
	#region Fields

	private static readonly EasClientDeviceInformation _deviceInformation;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public UwpRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public UwpRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	static UwpRuntimeInformation()
	{
		_deviceInformation = new EasClientDeviceInformation();

		Instance = new UwpRuntimeInformation();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationDataLocation()
	{
		return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
	}

	/// <summary>
	/// The elevated status of an application.
	/// </summary>
	protected override bool GetApplicationIsElevated()
	{
		return WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ?? false;
	}

	/// <inheritdoc />
	protected override string GetApplicationLocation()
	{
		var location = GetApplicationAssembly().Location;
		return Path.GetDirectoryName(location);
	}

	/// <inheritdoc />
	protected override string GetApplicationName()
	{
		return GetApplicationAssembly()?.GetName().Name;
	}

	/// <inheritdoc />
	protected override Version GetApplicationVersion()
	{
		return GetApplicationAssembly()?.GetName().Version;
	}

	/// <inheritdoc />
	protected override string GetDeviceId()
	{
		// Gets the publisher ID which is the same for all apps by this publisher on this device.
		var systemId = SystemIdentification.GetSystemIdForPublisher();

		// Make sure this device can generate the ID
		if (systemId.Source != SystemIdentificationSource.None)
		{
			return systemId.Id.ToArray().ToBase64String();
		}

		// Fallback to using EAS API
		return _deviceInformation.Id.ToString();
	}

	/// <inheritdoc />
	protected override string GetDeviceManufacturer()
	{
		return _deviceInformation.SystemManufacturer;
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		try
		{
			return _deviceInformation.SystemProductName;
		}
		catch
		{
			return "Unknown";
		}
	}

	/// <inheritdoc />
	protected override string GetDeviceName()
	{
		return Environment.MachineName;
	}

	/// <inheritdoc />
	protected override DevicePlatform GetDevicePlatform()
	{
		return DevicePlatform.Windows;
	}

	/// <inheritdoc />
	protected override Version GetDevicePlatformVersion()
	{
		var version = SystemInformation.Instance.OperatingSystemVersion;
		return new Version(version.Major, version.Minor, version.Build, version.Revision);
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		return DeviceType.Desktop;
	}

	#endregion
}