#region References

using System;
using System.IO;
using System.Security.Principal;
using Speedy.Application.Internal.Windows;
using Speedy.Application.Wpf.Extensions;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Wpf;

/// <inheritdoc />
public class WpfRuntimeInformation : RuntimeInformation
{
	#region Constructors

	/// <inheritdoc />
	public WpfRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public WpfRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	static WpfRuntimeInformation()
	{
		Instance = new WpfRuntimeInformation();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationDataLocation()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		return Path.Combine(localAppData, ApplicationName);
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
		return GetApplicationAssembly().GetAssemblyPath();
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
		return new DeviceId()
			.AddMachineName()
			.AddUserName()
			.AddMachineGuid()
			.AddSystemUuid()
			.AddMotherboardSerialNumber()
			.AddSystemDriveSerialNumber()
			.ToString();
	}

	/// <inheritdoc />
	protected override string GetDeviceManufacturer()
	{
		return new DeviceManufacturerRegistryComponent().GetValue()
			?? new DeviceManufacturerWmiComponent().GetValue();
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		return new DeviceModelRegistryComponent().GetValue()
			?? new DeviceModelWmiComponent().GetValue();
	}

	/// <inheritdoc />
	protected override string GetDeviceName()
	{
		return Environment.MachineName;
	}

	/// <inheritdoc />
	protected override DevicePlatform GetDevicePlatform()
	{
		#if (NET48_OR_GREATER)
		return DevicePlatform.Windows;
		#elif (NETCOREAPP3_1)
		return DevicePlatform.Windows;
		#else
		if (OperatingSystem.IsWindows())
		{
			return DevicePlatform.Windows;
		}

		if (OperatingSystem.IsAndroid())
		{
			return DevicePlatform.Android;
		}

		if (OperatingSystem.IsIOS())
		{
			return DevicePlatform.IOS;
		}

		if (OperatingSystem.IsLinux())
		{
			return DevicePlatform.Linux;
		}

		return DevicePlatform.Unknown;
		#endif
	}

	/// <inheritdoc />
	protected override Version GetDevicePlatformVersion()
	{
		return Environment.OSVersion.Version;
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		#if (NET48_OR_GREATER)
		return DeviceType.Desktop;
		#elif (NETCOREAPP3_1)
		return DeviceType.Desktop;
		#else
		if (OperatingSystem.IsAndroid()
			|| OperatingSystem.IsIOS())
		{
			return DeviceType.Phone;
		}

		return DeviceType.Desktop;
		#endif
	}

	#endregion
}