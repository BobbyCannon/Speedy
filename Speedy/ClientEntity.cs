#region References

using System;
using Speedy.Sync;

#endregion

namespace Speedy;

/// <inheritdoc cref="SyncModel{T}" />
public abstract class ClientEntity<T> : SyncModel<T>, IClientEntity
{
	#region Properties

	/// <inheritdoc />
	public DateTime LastClientUpdate { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void EntityAddedDeletedOrModified()
	{
		LastClientUpdate = TimeService.UtcNow;
		base.EntityAddedDeletedOrModified();
	}

	#endregion
}

/// <summary>
/// Represents a client entity.
/// </summary>
public interface IClientEntity
{
	#region Properties

	/// <summary>
	/// The last time the client entity was updated.
	/// </summary>
	public DateTime LastClientUpdate { get; set; }

	#endregion
}