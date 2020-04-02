#region References

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.Data.Client;
using Speedy.Extensions;

#endregion

namespace Speedy.IntegrationTests
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

				methods = database.Accounts.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
				sortMethod = methods.First(x => x.Name == "Sort");
				sortMethod.Invoke(database.Accounts, new object[0]);

				var address = database.Addresses.First();
				address.Accounts.Add(GetPerson());
				Assert.AreEqual(0, database.Accounts.Count());
				database.SaveChanges();
				Assert.AreEqual(1, database.Accounts.Count());
			}
		}

		private ClientAddress GetAddress()
		{
			return new ClientAddress
			{
				City = "City"
			};
		}

		private static ContosoClientMemoryDatabase GetDatabase(DatabaseOptions options = null)
		{
			return new ContosoClientMemoryDatabase(options);
		}

		private ClientAccount GetPerson()
		{
			return new ClientAccount
			{
				Name = "John"
			};
		}

		#endregion
	}
}