#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy.Application.Sync;

public class SyncableSetting<T> : SyncModel<T>
{
	#region Properties

	public string Category { get; set; }

	/// <summary>
	/// Optionally expires on value, DateTime.MinValue means there is no expiration.
	/// </summary>
	public DateTime ExpiresOn { get; set; }

	/// <inheritdoc />
	public override T Id { get; set; }

	public string Name { get; set; }

	/// <summary>
	/// Set to mark this setting as a syncable setting.
	/// </summary>
	public bool CanSync { get; set; }

	public string Value { get; set; }

	#endregion
}