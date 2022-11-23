#region References

using System;
using Speedy.Application.Xamarin.Extensions;
using Speedy.Data;
using Xamarin.Essentials;
using DevicePlatform = Speedy.Data.DevicePlatform;
using DeviceType = Speedy.Data.DeviceType;

#endregion

namespace Speedy.Application.Xamarin;

/// <inheritdoc />
public class XamarinRuntimeInformation : RuntimeInformation
{
	#region Constructors

	/// <inheritdoc />
	public XamarinRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public XamarinRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationName()
	{
		return AppInfo.Name;
	}

	/// <inheritdoc />
	protected override Version GetApplicationVersion()
	{
		var version = AppInfo.Version;
		var build = version.Build;
		var major = version.Major;
		var minor = version.Minor;
		var revision = version.Revision;

		// Mobile (Android) doesn't build the whole string.
		var hasBuild = int.TryParse(AppInfo.BuildString, out var buildNumber);
		if (hasBuild && ((revision == -1) || (revision != buildNumber)))
		{
			revision = buildNumber;
		}

		if ((major >= 0) && (minor >= 0) && (build >= 0) && (revision >= 0))
		{
			return new Version(major, minor, build, revision);
		}

		if ((major >= 0) && (minor >= 0) && (build >= 0))
		{
			return new Version(major, minor, build);
		}

		if ((major >= 0) && (minor >= 0))
		{
			return new Version(major, minor);
		}

		return new Version();
	}

	/// <inheritdoc />
	protected override string GetDeviceId()
	{
		return new DeviceId()
			.AddMachineName()
			.AddUserName()
			.AddVendorId()
			.ToString();
	}

	/// <inheritdoc />
	protected override string GetDeviceManufacturer()
	{
		return DeviceInfo.Manufacturer;
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		return DeviceInfo.Model;
	}

	/// <inheritdoc />
	protected override string GetDeviceName()
	{
		return DeviceInfo.Name;
	}

	/// <inheritdoc />
	protected override DevicePlatform GetDevicePlatform()
	{
		return DeviceInfo.Platform.ToDevicePlatform();
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		return DeviceInfo.Idiom.ToDeviceType();
	}

	#endregion
}