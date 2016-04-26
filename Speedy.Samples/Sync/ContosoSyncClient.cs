#region References

using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class ContosoSyncClient : IContosoSyncClient
	{
		#region Fields

		private readonly IContosoDatabaseProvider _provider;

		#endregion

		#region Constructors

		public ContosoSyncClient(string name, IContosoDatabaseProvider provider)
		{
			_provider = provider;
			Name = name;
		}

		#endregion

		#region Properties

		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public IEnumerable<SyncIssue> ApplyChanges(IEnumerable<SyncObject> changes)
		{
			return _provider.ApplySyncChanges(changes);
		}

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		public IEnumerable<SyncIssue> ApplyCorrections(IEnumerable<SyncObject> corrections)
		{
			return _provider.ApplySyncCorrections(corrections);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			return _provider.GetSyncChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			return _provider.GetSyncChanges(request);
		}

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		public IEnumerable<SyncObject> GetCorrections(IEnumerable<SyncIssue> issues)
		{
			return _provider.GetSyncCorrections(issues);
		}

		public IContosoDatabase GetDatabase()
		{
			return _provider.GetDatabase();
		}

		#endregion
	}
}