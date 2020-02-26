#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class UpdatableExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void UpdateShouldUpdateAllMembers()
		{
			var destination = new AccountEntity();
			var source = EntityFactory.GetAccount();

			source.Id = 99;
			source.SyncId = Guid.NewGuid();
			source.IsDeleted = true;
			source.Address.Id = 199;
			source.AddressId = 199;

			Assert.AreNotEqual(destination.Address, source.Address);
			Assert.AreNotEqual(destination.AddressId, source.AddressId);
			Assert.AreNotEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreNotEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreNotEqual(destination.Groups, source.Groups);
			Assert.AreNotEqual(destination.Id, source.Id);
			Assert.AreNotEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreNotEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreNotEqual(destination.Name, source.Name);
			Assert.AreNotEqual(destination.Pets, source.Pets);
			Assert.AreNotEqual(destination.SyncId, source.SyncId);

			// Update all members except virtual members
			UpdatableExtensions.UpdateWith(destination, source, typeof(AccountEntity).GetVirtualPropertyNames().ToArray());

			// All non virtual should be equal
			Assert.AreNotEqual(destination.Address, source.Address);
			Assert.AreEqual(destination.AddressId, source.AddressId);
			Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreNotEqual(destination.Groups, source.Groups);
			Assert.AreEqual(destination.Id, source.Id);
			Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreEqual(destination.Name, source.Name);
			Assert.AreNotEqual(destination.Pets, source.Pets);
			Assert.AreEqual(destination.SyncId, source.SyncId);

			// Update all members
			UpdatableExtensions.UpdateWith(destination, source);

			// All members should be equal now
			Assert.AreEqual(destination.Address, source.Address);
			Assert.AreEqual(destination.AddressId, source.AddressId);
			Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreEqual(destination.Groups, source.Groups);
			Assert.AreEqual(destination.Id, source.Id);
			Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreEqual(destination.Name, source.Name);
			Assert.AreEqual(destination.Pets, source.Pets);
			Assert.AreEqual(destination.SyncId, source.SyncId);
		}

		#endregion
	}
}