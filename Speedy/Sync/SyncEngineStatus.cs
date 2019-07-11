namespace Speedy.Sync
{
	/// <summary>
	/// Represents the different states of syncing.
	/// </summary>
	public enum SyncEngineStatus
	{
		/// <summary>
		/// The sync engine is not running.
		/// </summary>
		Stopped = 0,

		/// <summary>
		/// The sync engine is starting up.
		/// </summary>
		Starting = 1,

		/// <summary>
		/// The stage to pull data from the server and apply to the client.
		/// </summary>
		Pulling = 2,

		/// <summary>
		/// This stage is to push changes from the client and apply to the server.
		/// </summary>
		Pushing = 3,

		/// <summary>
		/// The sync engine was completed successfully.
		/// </summary>
		Completed = 4,

		/// <summary>
		/// The sync engine was cancelled.
		/// </summary>
		Cancelled = 5
	}
}