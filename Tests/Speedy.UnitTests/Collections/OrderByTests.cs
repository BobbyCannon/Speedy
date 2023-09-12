#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
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
	public void GetInsertIndex()
	{
		var scenarios = new List<(OrderBy<Account>[] order, (string[] existing, string insert, int expected)[] results)>
		{
			(
				new[]
				{
					new OrderBy<Account>(x => x.Name == "Zoe", true),
					new OrderBy<Account>(x => x.Name == "Zane", true),
					new OrderBy<Account>(x => x.Name)
				},
				new[]
				{
					(new[] { "Zane" }, "Zoe", 0),
					(new[] { "Zoe" }, "Zane", 1),
					(new[] { "Zoe", "Zane" }, "Bob", 2),
					(new[] { "Zoe", "Zane", "Jack" }, "Chad", 2)
				}
			)
		};

		for (var index = 0; index < scenarios.Count; index++)
		{
			$"Scenario {index}".Dump();

			var scenario = scenarios[index];

			foreach (var t in scenario.results)
			{
				$"{t.insert}".Dump();

				var insert = new Account { Name = t.insert };
				var existing = t.existing.Select(x => new Account { Name = x }).ToList();
				var actual = OrderBy<Account>.GetInsertIndex(existing, insert, scenario.order);

				AreEqual(t.expected, actual);
			}
		}
	}

	[TestMethod]
	public void InvalidDefaultOrderBy()
	{
		ExpectedException<InvalidOperationException>(
			() => _ = new OrderBy<SyncClient>(),
			"The type must implement IComparable to use this constructor."
		);
	}

	[TestMethod]
	public void OrderUsingManyOrderBy()
	{
		var scenarios = new List<(OrderBy<Account>[] order, (string[] unordered, string[] expected)[] results)>
		{
			(
				new[]
				{
					new OrderBy<Account>(x => x.Name == "Zoe", true),
					new OrderBy<Account>(x => x.Name == "Zane", true),
					new OrderBy<Account>(x => x.Name)
				},
				new[]
				{
					(new[] { "Zane", "Zoe" }, new[] { "Zoe", "Zane" }),
					(new[] { "Bob", "Zane", "Zoe" }, new[] { "Zoe", "Zane", "Bob" }),
					(new[] { "Bob", "Zane", "Joe", "Zoe" }, new[] { "Zoe", "Zane", "Bob", "Joe" })
				}
			)
		};

		for (var index = 0; index < scenarios.Count; index++)
		{
			$"Scenario {index}".Dump();

			var scenario = scenarios[index];

			foreach (var t in scenario.results)
			{
				var unordered = t.unordered.Select(x => new Account { Name = x }).ToList();
				var actual = OrderBy<Account>.OrderCollection(unordered, scenario.order);
				var expected = t.expected.Select(x => new Account { Name = x }).ToList();

				AreEqual(expected, actual);
			}
		}
	}

	#endregion
}