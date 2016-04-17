namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync state of an entity
	/// </summary>
	public enum SyncStatus
	{
		/// <summary>
		/// This entity was added.
		/// </summary>
		Added,

		/// <summary>
		/// This entity was last modified.
		/// </summary>
		Modified,

		/// <summary>
		/// This entity was delete.
		/// </summary>
		Deleted
	}
}