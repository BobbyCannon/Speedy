#region References

using System;
using System.IO;
using System.Reflection;
using Speedy.Data;

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
	protected override bool GetApplicationIsElevated()
	{
		return false;
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