namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync state of an entity
	/// </summary>
	public enum SyncConversionType
	{
		/// <summary>
		/// This entity is being added.
		/// </summary>
		Adding = 0,

		/// <summary>
		/// This entity is being modified.
		/// </summary>
		Modifying = 1,

		/// <summary>
		/// This entity is being deleted.
		/// </summary>
		Deleting = 2,

		/// <summary>
		/// This entity is being converted.
		/// </summary>
		Converting = 3
	}
}