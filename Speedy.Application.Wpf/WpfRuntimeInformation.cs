#region References

using System;
using System.Reflection;
using Speedy.Application.Wpf.Extensions;
using Speedy.Application.Wpf.Internal;
using Speedy.Data;

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

	#region Properties

	/// <summary>
	/// The global instance of the WPF runtime information.
	/// </summary>
	public static WpfRuntimeInformation Instance { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationName()
	{
		return Assembly.GetEntryAssembly()?.GetName().Name;
	}

	/// <inheritdoc />
	protected override Version GetApplicationVersion()
	{
		return Assembly.GetEntryAssembly()?.GetName().Version;
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
		return new ManufacturerDeviceIdComponent().GetValue();
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		return new ModelDeviceIdComponent().GetValue();
	}

	/// <inheritdoc />
	protected override string GetDeviceName()
	{
		return Environment.MachineName;
	}

	/// <inheritdoc />
	protected override DevicePlatform GetDevicePlatform()
	{
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
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		if (OperatingSystem.IsAndroid()
			|| OperatingSystem.IsIOS())
		{
			return DeviceType.Phone;
		}

		return DeviceType.Desktop;
	}

	#endregion
}