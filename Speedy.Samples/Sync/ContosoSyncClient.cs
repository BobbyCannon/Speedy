#region References

using System.Collections.Generic;
using System.Threading;
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
			Database = _provider.GetDatabase();
		}

		#endregion

		#region Properties

		public IContosoDatabase Database { get; }

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
			return Database.ApplySyncChanges(changes);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			return Database.GetSyncChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			return Database.GetSyncChanges(request);
		}

		public IContosoDatabase GetDatabase()
		{
			return _provider.GetDatabase();
		}

		public void SaveChanges()
		{
			Database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}