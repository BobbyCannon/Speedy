#region References

using System;
using System.Collections.Generic;
using System.Threading;
using Speedy.Samples.Entities;
using Speedy.Samples.Properties;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Sync
{
	public class SyncServer : ISyncServer
	{
		#region Fields

		private readonly EntityFrameworkContosoDatabase _database;

		#endregion

		#region Constructors

		public SyncServer(bool clearDatabase = true)
		{
			_database = new EntityFrameworkContosoDatabase();

			if (clearDatabase)
			{
				_database.Database.ExecuteSqlCommand(Resources.ClearDatabase);
			}
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => _database.Addresses;

		#endregion

		#region Methods

		public DateTime ApplyChanges(IEnumerable<SyncObject> changes)
		{
			_database.ApplySyncChanges(changes);
			SaveChanges();
			return DateTime.UtcNow;
		}

		public static SyncServer Create(Address address)
		{
			var server = new SyncServer();
			server.Addresses.Add(address.DeepClone(false));
			server.SaveChanges();
			return server;
		}

		public IEnumerable<SyncObject> GetChanges(DateTime since)
		{
			return _database.GetSyncChanges(since);
		}

		public void SaveChanges()
		{
			_database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}