namespace Speedy.Sync
{
	/// <summary>
	/// Represents the type of sync issue.
	/// </summary>
	public enum SyncIssueType
	{
		/// <summary>
		/// Could not determine the issue with the syncing object.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The sync item is having issue due to a relationship. Example another entity depends on the
		/// entity that is trying to be deleted. Another example is trying to sync an entity with a
		/// relationship to an entity that has not synced yet.
		/// </summary>
		RelationshipConstraint = 1,

		/// <summary>
		/// The sync item is having issue with a constraint (ex. Unique Index)
		/// </summary>
		ConstraintException = 2
	}
}