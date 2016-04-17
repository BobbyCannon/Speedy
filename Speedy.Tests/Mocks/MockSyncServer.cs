#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy.Tests.Properties;

#endregion

namespace Speedy.Tests.Mocks
{
	public class MockSyncServer : ISyncServer
	{
		#region Fields

		private readonly EntityFrameworkSampleDatabase _database;

		#endregion

		#region Constructors

		public MockSyncServer(bool clearDatabase = true)
		{
			_database = new EntityFrameworkSampleDatabase();

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

		public DateTime ApplyChanges(IEnumerable<SyncEntity> changes)
		{
			_database.SyncChanges(changes);
			SaveChanges();
			return DateTime.UtcNow;
		}

		public static MockSyncServer Create(Address address)
		{
			var server = new MockSyncServer();
			server.Addresses.Add(address.DeepClone(false));
			server.SaveChanges();
			return server;
		}

		public IEnumerable<SyncEntity> GetChanges(DateTime since)
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