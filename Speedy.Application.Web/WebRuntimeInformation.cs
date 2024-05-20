#region References

using System.IO;
using System.Security.Principal;
using Speedy.Runtime;

#if (NETFRAMEWORK)
using System.Web;
#else
using System;
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
	protected override string GetDeviceId()
	{
		return new DeviceId()
			.AddMachineName()
			.AddUserName()
			.ToString();
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