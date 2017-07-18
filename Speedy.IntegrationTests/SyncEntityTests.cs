#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SyncEntityTests
	{
		#region Methods

		[TestMethod]
		public void UpdateLocalSyncIds()
		{
			var address = new Address { Id = 1, SyncId = Guid.NewGuid() };
			var person = new Person { Name = "Bob", Address = address, AddressId = address.Id };

			person.UpdateLocalSyncIds();

			Assert.AreEqual(address.SyncId, person.AddressSyncId);
		}

		#endregion
	}
}