#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public class SyncClient : ISyncClient
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync client.
		/// </summary>
		public SyncClient(string name, ISyncableDatabaseProvider provider)
		{
			DatabaseProvider = provider;
			Name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the sync client.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the database provider.
		/// </summary>
		protected ISyncableDatabaseProvider DatabaseProvider { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public IEnumerable<SyncIssue> ApplyChanges(IEnumerable<SyncObject> changes)
		{
			return DatabaseProvider.ApplySyncChanges(changes);
		}

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		public IEnumerable<SyncIssue> ApplyCorrections(IEnumerable<SyncObject> corrections)
		{
			return DatabaseProvider.ApplySyncCorrections(corrections);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			return DatabaseProvider.GetSyncChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			return DatabaseProvider.GetSyncChanges(request);
		}

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		public IEnumerable<SyncObject> GetCorrections(IEnumerable<SyncIssue> issues)
		{
			return DatabaseProvider.GetSyncCorrections(issues);
		}

		#endregion
	}
}