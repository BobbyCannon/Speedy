#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.SyncApi;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class OrderByTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void DefaultOrderBy()
	{
		var list = new List<int> { 5, 3, 2, 4, 1 };
		var order = new OrderBy<int>();
		var actual = order.Process(list.AsQueryable());

		AreEqual(new[] { 1, 2, 3, 4, 5 }, actual.ToArray());
	}

	[TestMethod]
	public void DefaultOrderByForComparableObject()
	{
		var list = new List<Account>
		{
			new Account { Name = "John" },
			new Account { Name = "Jane" },
			new Account { Name = "Bob" }
		};
		var order = new OrderBy<Account>();
		var actual = order.Process(list.AsQueryable());

		AreEqual(new[] { list[2], list[1], list[0] }, actual.ToArray());
	}

	[TestMethod]
	public void DefaultOrderByForString()
	{
		var list = new List<string> { "C", "b", "A" };
		var order = new OrderBy<string>();
		var actual = order.Process(list.AsQueryable());

		AreEqual(new[] { "A", "b", "C" }, actual.ToArray());
	}

	[TestMethod]
	public void DefaultOrderByForStringNumbers()
	{
		var list = new List<string> { "11", "2", "1" };
		var order = new OrderBy<string>();
		var actual = order.Process(list.AsQueryable());

		AreEqual(new[] { "1", "11", "2" }, actual.ToArray());
	}

	[TestMethod]
	public void InvalidDefaultOrderBy()
	{
		ExpectedException<InvalidOperationException>(
			() => _ = new OrderBy<SyncClient>(),
			"The type must implement IComparable to use this constructor."
		);
	}

	#endregion
}