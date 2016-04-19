#region References

using System;
using System.Collections.Generic;
using System.Threading;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class SyncClient : ISyncClient
	{
		#region Fields

		private readonly ContosoDatabase _database;

		#endregion

		#region Constructors

		public SyncClient()
		{
			_database = new ContosoDatabase();
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => _database.Addresses;

		public DateTime LastSyncedOn { get; set; }

		public IRepository<Person> People => _database.People;

		#endregion

		#region Methods

		public void ApplyChanges(IEnumerable<SyncObject> changes)
		{
			_database.ApplySyncChanges(changes);
			SaveChanges();
		}

		public static SyncClient Create(Address item)
		{
			var client = new SyncClient();
			client.Addresses.Add(item.DeepClone(false));
			client.SaveChanges();
			return client;
		}

		public IEnumerable<SyncObject> GetChanges()
		{
			return _database.GetSyncChanges(LastSyncedOn);
		}

		public void SaveChanges()
		{
			_database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}