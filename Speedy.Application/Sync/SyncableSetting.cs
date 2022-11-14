#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Application.Sync;

/// <summary>
/// Represents a syncable setting.
/// </summary>
/// <typeparam name="T"> The type of the ID of the setting. </typeparam>
public class SyncableSetting<T> : SyncModel<T>
{
	#region Properties

	/// <summary>
	/// Set to mark this setting as a syncable setting.
	/// </summary>
	public bool CanSync { get; set; }

	/// <summary>
	/// The category for the settings.
	/// todo: should this be "group" instead?
	/// </summary>
	public string Category { get; set; }

	/// <summary>
	/// Optionally expires on value, DateTime.MinValue means there is no expiration.
	/// </summary>
	public DateTime ExpiresOn { get; set; }

	/// <inheritdoc />
	public override T Id { get; set; }

	/// <summary>
	/// The name of the setting.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The value of the setting in JSON format.
	/// </summary>
	public string Value { get; set; }

	#endregion
}