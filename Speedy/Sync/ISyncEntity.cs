#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public interface ISyncEntity : IModifiableEntity
	{
		#region Properties

		/// <summary>
		/// Used to communicate if the sync entity is deleted.
		/// </summary>
		bool IsDeleted { get; set; }

		/// <summary>
		/// The ID of the sync entity.
		/// </summary>
		/// <remarks>
		/// This ID is should be globally unique. Never reuse GUIDs.
		/// </remarks>
		Guid SyncId { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets the primary key (ID) of the sync entity.
		/// </summary>
		/// <returns> The primary key value for the sync entity. </returns>
		object GetEntityId();

		/// <summary>
		/// Gets the sync key (ID) of the sync entity. Defaults to SyncId.
		/// This can be overriden by setting the LookupFilter for a sync repository filter.
		/// </summary>
		/// <returns> The sync key value for the sync entity. </returns>
		object GetEntitySyncId();

		/// <summary>
		/// Checks a property to see if it can be synced in incoming data.
		/// </summary>
		/// <param name="propertyName"> The property name to be tested. </param>
		/// <returns> True if the property can be update during sync or false if otherwise. </returns>
		bool IsPropertyExcludedForIncomingSync(string propertyName);

		/// <summary>
		/// Checks a property to see if it can be synced in outgoing data.
		/// </summary>
		/// <param name="propertyName"> The property name to be tested. </param>
		/// <returns> True if the property can be update during sync or false if otherwise. </returns>
		bool IsPropertyExcludedForOutgoingSync(string propertyName);

		/// <summary>
		/// Checks a property has been excluded from updating.
		/// </summary>
		/// <param name="propertyName"> The property name to be tested. </param>
		/// <returns> True if the property can be written during an update or false if otherwise. </returns>
		bool IsPropertyExcludedForSyncUpdate(string propertyName);

		/// <summary>
		/// Converts the entity into an object to transmit.
		/// </summary>
		/// <returns> The sync object for this entity. </returns>
		SyncObject ToSyncObject();

		/// <summary>
		/// Update all local sync IDs.
		/// </summary>
		void UpdateLocalSyncIds();

		/// <summary>
		/// Updates the entity with the provided entity. Virtual properties will be ignored.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
		void UpdateWith(ISyncEntity update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate);

		#endregion
	}
}