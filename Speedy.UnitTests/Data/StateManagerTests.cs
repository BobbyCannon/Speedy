#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data;

#endregion

namespace Speedy.UnitTests.Data;

[TestClass]
public class StateManagerTests : SpeedyUnitTest
{
	[TestMethod]
	public void BasicStateComparerWithValueType()
	{
		var comparer = new BasicComparer<int>((c, u) => u > c, (c, u) => (u, true));

		Assert.AreEqual(0, comparer.CurrentValue);

		(int update, int expected)[] scenarios = {
			// Refreshing with 2 from a state of 0 should update
			(2, 2), 
			// Refreshing with 1 from a state of 2 should not update
			(1, 2),
			// Refreshing with 1024 from a state of 2 should update
			(1024, 1024),
			// Refreshing with -1 from a state of 1024 should update
			(-1, 1024),
		};

		foreach (var scenario in scenarios)
		{
			$"{comparer.CurrentValue} updating with {scenario.update} and expect {scenario.expected}".Dump();

			comparer.Refresh(scenario.update);
			Assert.AreEqual(scenario.expected, comparer.CurrentValue);
		}
	}
}