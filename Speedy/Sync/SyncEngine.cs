namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync engine.
	/// </summary>
	public static class SyncEngine
	{
		#region Methods

		/// <summary>
		/// Pull changes from the server and apply them to the client. Pushes changes to the server.
		/// </summary>
		/// <param name="client"> The client. </param>
		/// <param name="server"> The server. </param>
		public static void PullAndPushChanges(ISyncClient client, ISyncServer server)
		{
			PullChanges(client, server);
			PushChanges(client, server);
		}

		/// <summary>
		/// Pull changes from the server and apply them to the client.
		/// </summary>
		/// <param name="client"> The client. </param>
		/// <param name="server"> The server. </param>
		public static void PullChanges(ISyncClient client, ISyncServer server)
		{
			var changes = server.GetChanges(client.LastSyncedOn);
			if (changes == null)
			{
				return;
			}

			client.ApplyChanges(changes);
		}

		/// <summary>
		/// Push changes to the server.
		/// </summary>
		/// <param name="client"> The client. </param>
		/// <param name="server"> The server. </param>
		public static void PushChanges(ISyncClient client, ISyncServer server)
		{
			var entities = client.GetChanges();
			if (entities == null)
			{
				return;
			}

			client.LastSyncedOn = server.ApplyChanges(entities);
		}

		#endregion
	}
}