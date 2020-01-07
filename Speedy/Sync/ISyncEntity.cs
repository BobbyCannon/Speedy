#region References

using System;
using System.Collections.Generic;

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
		/// Add a property to exclude during sync for incoming data.
		/// </summary>
		/// <param name="propertyNames"> The names of the property to exclude. </param>
		void ExcludePropertiesForIncomingSync(params string[] propertyNames);

		/// <summary>
		/// Get the properties excluded during sync for incoming data.
		/// </summary>
		/// <returns> The names of the property to exclude. </returns>
		HashSet<string> GetExcludedPropertiesForIncomingSync();
		
		/// <summary>
		/// Add a property to exclude during sync for outgoing data.
		/// </summary>
		/// <param name="propertyNames"> The names of the property to exclude. </param>
		void ExcludePropertiesForOutgoingSync(params string[] propertyNames);

		/// <summary>
		/// Get the properties excluded during sync for outgoing data.
		/// </summary>
		/// <returns> The names of the property to exclude. </returns>
		HashSet<string> GetExcludedPropertiesForOutgoingSync();

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
		/// Reset the ID back to it's default
		/// </summary>
		void ResetId();

		/// <summary>
		/// Resets incoming sync exclusion back to default values or just clears if setToDefault is false.
		/// </summary>
		/// <param name="setToDefault"> Set to default excluded values. Defaults to true. </param>
		void ResetExcludedPropertiesForIncomingSync(bool setToDefault = true);
		
		/// <summary>
		/// Resets outgoing sync exclusion back to default values or just clears if setToDefault is false.
		/// </summary>
		/// <param name="setToDefault"> Set to default excluded values. Defaults to true. </param>
		void ResetExcludedPropertiesForOutgoingSync(bool setToDefault = true);

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
		/// Updates the entity with the provided entity.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		void UpdateWith(ISyncEntity update, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForUpdate);

		/// <summary>
		/// Updates the entity with the provided entity.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="exclusions"> Excluded properties will not be set during update. </param>
		void UpdateWith(ISyncEntity update, params string[] exclusions);

		#endregion
	}
}