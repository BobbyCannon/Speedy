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
		/// The sync item is having issue with a constraint (ex. Unique Index).
		/// </summary>
		ConstraintException = 2,

		/// <summary>
		/// The item is not being processed because this repository is being filtered.
		/// </summary>
		RepositoryFiltered = 3,

		/// <summary>
		/// The item is not being processed because the sync entity is being filtered.
		/// </summary>
		SyncEntityFiltered = 4,

		/// <summary>
		/// The item is not being updated due to an update exception.
		/// </summary>
		UpdateException = 5,

		/// <summary>
		/// There was an exception with one of the sync clients.
		/// </summary>
		ClientException = 6,

		/// <summary>
		/// The sync client was not authorized to access the server.
		/// </summary>
		Unauthorized = 7,

		/// <summary>
		/// The client is not supported by this server.
		/// </summary>
		ClientNotSupported = 8
	}
}