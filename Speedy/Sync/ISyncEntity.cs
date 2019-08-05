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
		/// Add a property to exclude during sync.
		/// </summary>
		/// <param name="propertyNames"> The names of the property to exclude. </param>
		void ExcludePropertiesForSync(params string[] propertyNames);

		/// <summary>
		/// Checks a property to see if it can be synced.
		/// </summary>
		/// <param name="propertyName"> The property name to be tested. </param>
		/// <returns> True if the property can be update during sync or false if otherwise. </returns>
		bool IsPropertyExcludedForSync(string propertyName);

		/// <summary>
		/// Resets exclusion back to default values or just clears if setToDefault is false.
		/// </summary>
		/// <param name="setToDefault"> Set to default excluded values. Defaults to true. </param>
		void ResetPropertySyncExclusions(bool setToDefault = true);

		/// <summary>
		/// Reset the ID back to it's default
		/// </summary>
		void ResetId();

		/// <summary>
		/// Converts the entity into an object to transmit.
		/// </summary>
		/// <returns> The sync object for this entity. </returns>
		SyncObject ToSyncObject();

		/// <summary>
		/// Updates the entity with the provided entity.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="allowSyncExclusions"> If true excluded sync properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="allowUpdateExclusions"> If true excluded update properties will not be updated otherwise all matching properties will be updated. </param>
		void UpdateWith(ISyncEntity update, bool allowSyncExclusions = true, bool allowUpdateExclusions = true);

		/// <summary>
		/// Update all local sync IDs.
		/// </summary>
		void UpdateLocalSyncIds();

		#endregion
	}
}