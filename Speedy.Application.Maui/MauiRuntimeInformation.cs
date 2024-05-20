#region References

using Speedy.Application.Maui.Extensions;
using Speedy.Runtime;
using DevicePlatform = Speedy.Runtime.DevicePlatform;
using DeviceType = Speedy.Runtime.DeviceType;

#endregion

namespace Speedy.Application.Maui;

/// <inheritdoc />
public class MauiRuntimeInformation : RuntimeInformation
{
	#region Constructors

	/// <inheritdoc />
	public MauiRuntimeInformation() : this(null)
	{
	}

	/// <inheritdoc />
	public MauiRuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	static MauiRuntimeInformation()
	{
		Instance = new MauiRuntimeInformation();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override string GetApplicationDataLocation()
	{
		#if (WINDOWS)
		var location = GetApplicationAssembly().Location;
		return Path.GetDirectoryName(location);
		#else
		return FileSystem.Current.AppDataDirectory;
		#endif
	}

	/// <inheritdoc />
	protected override bool GetApplicationIsElevated()
	{
		return false;
	}

	/// <inheritdoc />
	protected override string GetApplicationLocation()
	{
		return FileSystem.Current.AppDataDirectory;
	}

	/// <inheritdoc />
	protected override string GetApplicationName()
	{
		return AppInfo.Current.Name;
	}

	/// <inheritdoc />
	protected override Version GetApplicationVersion()
	{
		#if (WINDOWS)
		var assemblyName = GetApplicationAssembly()?.GetName();
		if (assemblyName != null)
		{
			return assemblyName.Version;
		}
		#endif

		var version = AppInfo.Current.Version;
		var build = version.Build;
		var major = version.Major;
		var minor = version.Minor;
		var revision = version.Revision;

		// Mobile (Android) doesn't build the whole string.
		var hasBuild = int.TryParse(AppInfo.Current.BuildString, out var buildNumber);
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
		return DeviceInfo.Current.Manufacturer;
	}

	/// <inheritdoc />
	protected override string GetDeviceModel()
	{
		return DeviceInfo.Current.Model;
	}

	/// <inheritdoc />
	protected override string GetDeviceName()
	{
		return DeviceInfo.Current.Name;
	}

	/// <inheritdoc />
	protected override DevicePlatform GetDevicePlatform()
	{
		return DeviceInfo.Current.Platform.ToDevicePlatform();
	}

	/// <inheritdoc />
	protected override Version GetDevicePlatformVersion()
	{
		return DeviceInfo.Current.Version;
	}

	/// <inheritdoc />
	protected override DeviceType GetDeviceType()
	{
		return DeviceInfo.Current.Idiom.ToDeviceType();
	}

	#endregion
}