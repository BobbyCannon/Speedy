#region References

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Net;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy.Tests.EntityFactories;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Models;

#endregion

namespace Speedy.Samples.Tests
{
	[TestClass]
	public class ContosoModelDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void SortShouldNotBreakLocalRepository()
		{
			using (var database = GetDatabase())
			{
				database.Addresses.Add(GetAddress());
				Assert.AreEqual(0, database.Addresses.Count());
				database.SaveChanges();
				Assert.AreEqual(1, database.Addresses.Count());

				var methods = database.Addresses.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
				var sortMethod = methods.First(x => x.Name == "Sort");
				sortMethod.Invoke(database.Addresses, new object[0]);

				methods = database.People.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
				sortMethod = methods.First(x => x.Name == "Sort");
				sortMethod.Invoke(database.People, new object[0]);

				var address = database.Addresses.First();
				address.People.Add(GetPerson());
				Assert.AreEqual(0, database.People.Count());
				database.SaveChanges();
				Assert.AreEqual(1, database.People.Count());
			}
		}

		[TestMethod]
		public void SyncOnlyOnePersonsAddresses()
		{
			var server = new WebSyncClient("Server (WEB)", TestHelper.GetSyncableSqlProvider(), "https://speedy.local", "api/ModelSync", timeout: 60000);
			var client = new SyncClient("Client (MEM)", GetSyncableModelDatabaseProvider());
			PersonEntity person;

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				person = new PersonEntity { Name = "John", Address = AddressFactory.Get(), SyncId = Guid.NewGuid() };
				database.People.Add(person);
				database.SaveChanges();
			}

			var options = new SyncOptions();
			options.AddSyncableFilter(new SyncRepositoryFilter<Address>(x => x.People.Any(y => y.SyncId == person.SyncId)));
			options.AddSyncableFilter(new SyncRepositoryFilter<Person>(x => x.SyncId == person.SyncId));
			options.Values.Add("Filter", $"Person={person.SyncId}");

			var engine = new SyncEngine(client, server, options);
			engine.Run();

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, database.Addresses.Count());
				Assert.AreEqual(1, database.People.Count());
			}

			using (var database = client.GetDatabase<ContosoModelDatabase>())
			{
				Assert.AreEqual(1, database.Addresses.Count());
				Assert.AreEqual(1, database.People.Count());
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableModelDatabaseProvider(DatabaseOptions options = null)
		{
			var database = GetDatabase(options);
			return new SyncDatabaseProvider(x => database, database.Options);
		}

		private Address GetAddress()
		{
			return new Address
			{
				City = "City"
			};
		}

		private static ContosoModelDatabase GetDatabase(DatabaseOptions options = null)
		{
			return new ContosoModelDatabase(options);
		}

		private Person GetPerson()
		{
			return new Person
			{
				Name = "John"
			};
		}

		#endregion
	}
}