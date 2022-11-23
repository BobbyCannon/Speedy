#region References

using System;
using System.Linq;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync;

/// <summary>
/// The sync client details.
/// </summary>
public class SyncClientDetails : Bindable<ISyncClientDetails>, ISyncClientDetails
{
	#region Properties

	/// <inheritdoc />
	public string ApplicationName { get; set; }

	/// <inheritdoc />
	public Version ApplicationVersion { get; set; }

	/// <inheritdoc />
	public string DeviceId { get; set; }

	/// <inheritdoc />
	public DevicePlatform DevicePlatform { get; set; }

	/// <inheritdoc />
	public DeviceType DeviceType { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			SyncClientDetails entityUpdate => UpdateWith(entityUpdate, exclusions),
			ISyncClientDetails entityUpdate => UpdateWith(entityUpdate, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <inheritdoc />
	public override bool UpdateWith(ISyncClientDetails update, params string[] exclusions)
	{
		// ****** You can use CodeGeneratorTests.GenerateProperties to update this ******

		if (exclusions == null)
		{
			this.IfThen(e => e.ApplicationName != update.ApplicationName, e => e.ApplicationName = update.ApplicationName);
			this.IfThen(e => e.ApplicationVersion != update.ApplicationVersion, e => e.ApplicationVersion = update.ApplicationVersion);
			this.IfThen(e => e.DeviceId != update.DeviceId, e => e.DeviceId = update.DeviceId);
			this.IfThen(e => e.DevicePlatform != update.DevicePlatform, e => e.DevicePlatform = update.DevicePlatform);
			this.IfThen(e => e.DeviceType != update.DeviceType, e => e.DeviceType = update.DeviceType);
		}
		else
		{
			this.IfThen(e => !exclusions.Contains(nameof(ApplicationName)) && (e.ApplicationName != update.ApplicationName), e => e.ApplicationName = update.ApplicationName);
			this.IfThen(e => !exclusions.Contains(nameof(ApplicationVersion)) && (e.ApplicationVersion != update.ApplicationVersion), e => e.ApplicationVersion = update.ApplicationVersion);
			this.IfThen(e => !exclusions.Contains(nameof(DeviceId)) && (e.DeviceId != update.DeviceId), e => e.DeviceId = update.DeviceId);
			this.IfThen(e => !exclusions.Contains(nameof(DevicePlatform)) && (e.DevicePlatform != update.DevicePlatform), e => e.DevicePlatform = update.DevicePlatform);
			this.IfThen(e => !exclusions.Contains(nameof(DeviceType)) && (e.DeviceType != update.DeviceType), e => e.DeviceType = update.DeviceType);
		}

		return true;
	}

	#endregion
}

/// <summary>
/// The details for a sync client.
/// </summary>
public interface ISyncClientDetails
{
	#region Properties

	/// <summary>
	/// The ApplicationName value for Sync Client Details.
	/// </summary>
	string ApplicationName { get; set; }

	/// <summary>
	/// The ApplicationVersion value for Sync Client Details.
	/// </summary>
	Version ApplicationVersion { get; set; }

	/// <summary>
	/// The DeviceId value for Sync Client Details.
	/// </summary>
	string DeviceId { get; set; }

	/// <summary>
	/// The DevicePlatform value for Sync Client Details.
	/// </summary>
	DevicePlatform DevicePlatform { get; set; }

	/// <summary>
	/// The DeviceType value for Sync Client Details.
	/// </summary>
	DeviceType DeviceType { get; set; }

	#endregion
}