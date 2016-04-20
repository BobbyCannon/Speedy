#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class ContosoDatabaseSyncClient : ISyncClient
	{
		#region Fields

		private readonly IContosoDatabase _database;

		#endregion

		#region Constructors

		public ContosoDatabaseSyncClient(string name, IContosoDatabase database)
		{
			Name = name;
			_database = database;
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => _database.Addresses;

		public string Name { get; }

		public IRepository<Person> People => _database.People;

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public void ApplyChanges(IEnumerable<SyncObject> changes)
		{
			_database.ApplySyncChanges(changes);
			SaveChanges();
		}

		public static ContosoDatabaseSyncClient Create(string name, IContosoDatabase database, Address item)
		{
			var client = new ContosoDatabaseSyncClient(name, database);
			client.Addresses.Add(item.DeepClone(false));
			client.SaveChanges();
			return client;
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			return _database.GetSyncChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			return _database.GetSyncChanges(request);
		}

		public void SaveChanges()
		{
			_database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}