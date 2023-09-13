#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data;
using Speedy.Data.Views;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class FlattenHierarchicalListTests : BaseCollectionTests
{
	#region Methods

	[TestMethod]
	public void AddingChildToAnotherParentShouldRemoveFromOriginalParent()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);
		actualList.ManageRelationships = true;

		var account1 = Activator.CreateInstance<AccountView>(x => x.Name = "John");
		var account2 = Activator.CreateInstance<AccountView>(x => x.Name = "Jane");
		var address = Activator.CreateInstance<AddressView>(x => x.Line1 = "123 Main Street");

		accountList.Add(account1);
		account1.Addresses.Add(address);
		accountList.Add(account2);
		
		AreEqual(new IHierarchyListItem[] { account2, account1, address }, actualList.ToArray());
		AreEqual(1, account1.Addresses.Count);
		AreEqual(0, account2.Addresses.Count);
		AreEqual(account1, address.GetParent());

		// Just assigning the address to account2 should correct all relationships.
		account2.Addresses.Add(address);

		AreEqual(new IHierarchyListItem[] { account2, address, account1 }, actualList.ToArray());
		AreEqual(0, account1.Addresses.Count);
		AreEqual(1, account2.Addresses.Count);
		AreEqual(account2, address.GetParent());
	}

	[TestMethod]
	public void ChildrenShouldBeRemovedWhenRemovedFromParent()
	{
		using var accountList = GetAccountsList();
		using var list = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		var address = Activator.CreateInstance<AddressView>();
		account.Addresses.Add(address);
		accountList.Add(account);
		
		AreEqual(2, list.Count);
		AreEqual(new IHierarchyListItem[] { account, address }, list.ToArray());

		account.Addresses.Remove(address);

		AreEqual(1, list.Count);
		AreEqual(new IHierarchyListItem[] { account }, list.ToArray());
	}

	[TestMethod]
	public void ChildrenShouldSortEvenIfAllParentsNotShown()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);

		var account1 = Activator.CreateInstance<AccountView>(x =>
		{
			x.Name = "John";
			x.IsShown = false;
		});
		var account2 = Activator.CreateInstance<AccountView>(x =>
		{
			x.Name = "Jane";
			x.IsShown = false;
		});
		var address1 = Activator.CreateInstance<AddressView>(x => x.Line1 = "123 Main Street");
		account1.Addresses.Add(address1);
		accountList.Add(account1);
		accountList.Add(account2);

		AreEqual(new IHierarchyListItem[] { address1 }, actualList.ToArray());

		var address2 = Activator.CreateInstance<AddressView>(x => x.Line1 = "145 Main Street");
		account2.Addresses.Add(address2);

		AreEqual(new IHierarchyListItem[] { address2, address1 }, actualList.ToArray());
	}

	[TestMethod]
	public void CollectionShouldResortOnValueChanges()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);

		var account1 = Activator.CreateInstance<AccountView>();
		account1.Name = "John";
		accountList.Add(account1);

		var account2 = Activator.CreateInstance<AccountView>();
		account2.Name = "Jane";
		accountList.Add(account2);

		AreEqual(new IHierarchyListItem[] { account2, account1 }, actualList.ToArray());

		var account2Address1 = Activator.CreateInstance<AddressView>();
		account2Address1.Line1 = "123 Main Street";
		account2.Addresses.Add(account2Address1);

		AreEqual(new IHierarchyListItem[] { account2, account2Address1, account1 }, actualList.ToArray());
	}

	[TestMethod]
	public void ParentCanHideWhileStillShowingChildren()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		accountList.Add(account);
		var address = Activator.CreateInstance<AddressView>();
		account.Addresses.Add(address);

		AreEqual(2, actualList.Count);
		AreEqual(new IHierarchyListItem[] { account, address }, actualList.ToArray());

		account.IsShown = false;

		AreEqual(1, actualList.Count);
		AreEqual(new IHierarchyListItem[] { address }, actualList.ToArray());
	}

	[TestMethod]
	public void ReassignParent()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);
		actualList.ManageRelationships = true;

		// Adding account 1 to the list should add to list.
		var account1 = Activator.CreateInstance<AccountView>(x => x.Name = "John");
		accountList.Add(account1);
		AreEqual(new IHierarchyListItem[] { account1 }, actualList.ToArray());

		// Adding account 2 to the list should add to list but in the first index.
		var account2 = Activator.CreateInstance<AccountView>(x => x.Name = "Jane");
		accountList.Add(account2);
		AreEqual(new IHierarchyListItem[] { account2, account1 }, actualList.ToArray());

		// Adding address to account2 should insert after it.
		var address = Activator.CreateInstance<AddressView>(x => x.Line1 = "123 Main Street");
		AreEqual(null, address.GetParent());
		account2.Addresses.Add(address);
		AreEqual(account2, address.GetParent());
		AreEqual(new IHierarchyListItem[] { account2, address, account1 }, actualList.ToArray());

		// Removing the address should remove it from the list.
		AreEqual(account2, address.GetParent());
		account2.Addresses.Remove(address);
		AreEqual(new IHierarchyListItem[] { account2, account1 }, actualList.ToArray());
		AreEqual(null, address.GetParent());

		// Adding the address to the first account it should come back into the list.
		account1.Addresses.Add(address);
		AreEqual(new IHierarchyListItem[] { account2, account1, address }, actualList.ToArray());

		// This should not hide the address.
		account2.ShowChildren = false;
		AreEqual(new IHierarchyListItem[] { account2, account1, address }, actualList.ToArray());

		// This should hide the address.
		account1.ShowChildren = false;
		AreEqual(new IHierarchyListItem[] { account2, account1 }, actualList.ToArray());
	}

	[TestMethod]
	public void ShouldAllowChildrenToBeAdded()
	{
		using var accountList = GetAccountsList();
		using var list = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		accountList.Add(account);

		AreEqual(1, list.Count);
		AreEqual(new IHierarchyListItem[] { account }, list.ToArray());

		var address = Activator.CreateInstance<AddressView>();
		account.Addresses.Add(address);

		AreEqual(2, list.Count);
		AreEqual(new IHierarchyListItem[] { account, address }, list.ToArray());
	}

	[TestMethod]
	public void ShouldAllowChildrenToToBeHidden()
	{
		using var accountList = GetAccountsList();
		using var list = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		accountList.Add(account);
		var address = Activator.CreateInstance<AddressView>();
		account.Addresses.Add(address);

		AreEqual(2, list.Count);
		AreEqual(new IHierarchyListItem[] { account, address }, list.ToArray());

		address.IsShown = false;

		AreEqual(1, list.Count);
		AreEqual(new IHierarchyListItem[] { account }, list.ToArray());
	}

	[TestMethod]
	public void ShouldBeAddedWhenShown()
	{
		using var accountList = GetAccountsList();
		using var list = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		account.IsShown = false;
		accountList.Add(account);

		AreEqual(0, list.Count);

		account.IsShown = true;

		AreEqual(1, list.Count);
		AreEqual(account, list[0]);
	}

	[TestMethod]
	public void ShouldBeRemovedWhenHidden()
	{
		using var accountList = GetAccountsList();
		using var actualList = new FlattenHierarchicalList(accountList);

		var account = Activator.CreateInstance<AccountView>();
		accountList.Add(account);

		AreEqual(1, actualList.Count);
		AreEqual(account, accountList[0]);

		account.IsShown = false;

		AreEqual(0, actualList.Count);
	}

	[TestMethod]
	public void ShouldDisposeChildren()
	{
		using var accountList = GetAccountsList();
		var eventInfo = typeof(SpeedyList<AccountView>).GetCachedEventField(nameof(accountList.CollectionChanged));
		var eventValue = eventInfo.GetValue(accountList);
		IsNull(eventValue);

		var actualList = new FlattenHierarchicalList(accountList);
		var account = Activator.CreateInstance<AccountView>();
		accountList.Add(account);

		AreEqual(1, actualList.Count);
		eventValue = eventInfo.GetValue(accountList);
		IsNotNull(eventValue);

		actualList.Dispose();
		eventValue = eventInfo.GetValue(accountList);
		IsNull(eventValue);
	}

	[TestMethod]
	public void ShowingManyChildrenShouldOnlyOrderOnce()
	{
		using var accountList = GetAccountsList();
		using var list = new FlattenHierarchicalList(accountList);
		list.ManageRelationships = true;

		var account1 = Add(accountList, Activator.CreateInstance<AccountView>(x => x.Name = "Jane"));
		var account2 = Add(accountList, Activator.CreateInstance<AccountView>(x => x.Name = "John"));
		account1.ShowChildren = false;
		list.InitializeProfiler();

		AreEqual(new IHierarchyListItem[] { account1, account2 }, list.ToArray());
		AreEqual(0, list.Profiler.OrderCount.Value);

		var address4 = Add(account1.Addresses, Activator.CreateInstance<AddressView>(x => x.Line1 = "104 Main Street"));
		var address2 = Add(account1.Addresses, Activator.CreateInstance<AddressView>(x => x.Line1 = "102 Main Street"));
		var address3 = Add(account1.Addresses, Activator.CreateInstance<AddressView>(x => x.Line1 = "103 Main Street"));
		var address1 = Add(account1.Addresses, Activator.CreateInstance<AddressView>(x => x.Line1 = "101 Main Street"));

		AreEqual(new IHierarchyListItem[] { account1, account2 }, list.ToArray());
		AreEqual(0, list.Profiler.OrderCount.Value);

		// This should add all 4 address to the flatten list but only order once.
		account1.ShowChildren = true;

		AreEqual(new IHierarchyListItem[] { account1, address1, address2, address3, address4, account2 }, list.ToArray());
		AreEqual(1, list.Profiler.OrderCount.Value);
	}

	private SpeedyList<AccountView> GetAccountsList()
	{
		return new SpeedyList<AccountView>(new OrderBy<AccountView>(x => x.Name));
	}

	#endregion
}