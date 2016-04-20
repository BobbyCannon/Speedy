#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Samples.Sync;
using Speedy.Sync;
using Speedy.Tests.Properties;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class SyncServerClientTests
	{
		#region Methods

		[TestMethod]
		public void SyncEngineAddItemToClient()
		{
			using (var database = new EntityFrameworkContosoDatabase())
			{
				database.ClearDatabase();

				var address = NewAddress("Foo");
				var person = new Person { Address = address, Name = "John Smith" };

				database.People.Add(person);
				database.SaveChanges();
			}

			var client = new ContosoDatabaseSyncClient("MEM", new ContosoDatabase());
			var server = new WebSyncClient("http://localhost");

			client.Addresses.Add(NewAddress("Blah"));
			client.SaveChanges();

			var engine = new SyncEngine(client, server, DateTime.MinValue); engine.Run();
			client.SaveChanges();

			using (var serverDatabase = new EntityFrameworkContosoDatabase())
			{
				Assert.AreEqual(2, client.Addresses.Count());
				Assert.AreEqual(1, client.People.Count());
				Assert.AreEqual("Foo", client.People.First().Address.Line1);
				Assert.AreEqual(2, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, serverDatabase.People.Count());
				Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
			}
		}

		private EntityFrameworkContosoDatabase GetDatabase()
		{
			var response = new EntityFrameworkContosoDatabase();
			return response;
		}

		private Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		#endregion
	}
}