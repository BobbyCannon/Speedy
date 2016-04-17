#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Tests.Mocks
{
	public class MockSyncClient : ISyncClient
	{
		#region Fields

		private readonly SampleDatabase _database;

		#endregion

		#region Constructors

		public MockSyncClient()
		{
			_database = new SampleDatabase();
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => _database.Addresses;

		public DateTime LastSyncedOn { get; set; }

		#endregion

		#region Methods

		public void ApplyChanges(IEnumerable<SyncEntity> changes)
		{
			_database.SyncChanges(changes);
			SaveChanges();
		}

		public static MockSyncClient Create(Address item)
		{
			var client = new MockSyncClient();
			client.Addresses.Add(item.DeepClone(false));
			client.SaveChanges();
			return client;
		}

		public IEnumerable<SyncEntity> GetChanges()
		{
			return _database.GetReadOnlyRepository<Address>()
				.Where(x => x.ModifiedOn > LastSyncedOn)
				.ToList()
				.Select(x => x.DeepClone(true))
				.ToList();
		}

		public void SaveChanges()
		{
			_database.SaveChanges();
			Thread.Sleep(1);
		}

		#endregion
	}
}