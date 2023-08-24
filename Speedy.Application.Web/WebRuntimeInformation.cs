#region References

using System;
using System.IO;
using System.Security.Principal;
using Speedy.Application.Internal.Windows;
using Speedy.Data;
#if (NETFRAMEWORK)
using System.Web;
#endif

#endregion

namespace Speedy.Application.Web;

/// <inheritdoc />
public class WebRuntimeInformation : RuntimeInformation
{
	#region Constructors

	/// <inheritdoc />
	public WebRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public WebRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	static WebRuntimeInformation()
	{
		Instance = new WebRuntimeInformation();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationDataLocation()
	{
		#if (NETFRAMEWORK)
		var localAppData = HttpContext.Current.Server.MapPath("~/App_Data");
		return Path.Combine(localAppData, ApplicationName);
		#else
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		return Path.Combine(localAppData, ApplicationName);
		#endif
	}

	/// <summary>
	/// The elevated status of an application.
	/// </summary>
	protected override bool GetApplicationIsElevated()
	{
		return IsWindows() && (WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ?? false);
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
		return new DeviceId()
			.AddMachineName()
			.AddUserName()
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
		return DevicePlatform.Windows;
	}

	/// <inheritdoc />
	protected override Version GetDevicePlatformVersion()
	{
		return Environment.OSVersion.Version;
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		return DeviceType.Desktop;
	}

	#endregion
}