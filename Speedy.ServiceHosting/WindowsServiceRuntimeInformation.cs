#region References

using System;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using Speedy.Application;
using Speedy.Data;

#endregion

namespace Speedy.ServiceHosting;

/// <inheritdoc />
public class WindowsServiceRuntimeInformation : RuntimeInformation
{
	#region Constructors

	/// <inheritdoc />
	public WindowsServiceRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public WindowsServiceRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	static WindowsServiceRuntimeInformation()
	{
		Instance = new WindowsServiceRuntimeInformation();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override bool GetApplicationIsElevated()
	{
		return WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ?? false;
	}

	/// <inheritdoc />
	protected override string GetApplicationLocation()
	{
		var location = Assembly.GetEntryAssembly()?.Location
			?? Assembly.GetCallingAssembly().Location;

		return Path.GetDirectoryName(location);
	}

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
			.ToString();
	}

	/// <inheritdoc />
	protected override string GetDeviceManufacturer()
	{
		return string.Empty;
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		return string.Empty;
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
	protected override DeviceType GetDeviceType()
	{
		return DeviceType.Desktop;
	}

	#endregion
}