#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Collections;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class FilteredCollectionTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void FilterCollectionShouldFilter()
	{
		var filteredCount = 0;
		var originalCollection = new SortedObservableCollection<int>(new OrderBy<int>(x => x));
		var thirdsOnly = new FilteredCollection<int>(originalCollection, x => (x % 3) == 0);
		thirdsOnly.CollectionChanged += (_, _) => filteredCount++;

		for (var i = 1; i <= 10; i++)
		{
			originalCollection.Add(i);
		}

		// We should have only been alerted with anything divisible by 3
		AreEqual(3, thirdsOnly.Count);
		AreEqual(new[] { 3, 6, 9 }, thirdsOnly.ToArray());
		AreEqual(3, filteredCount);

		originalCollection.Remove(3);

		// Filter collection should have changed
		AreEqual(2, thirdsOnly.Count);
		AreEqual(new[] { 6, 9 }, thirdsOnly.ToArray());
		AreEqual(4, filteredCount);

		originalCollection.Remove(1);
		originalCollection.Remove(2);
		originalCollection.Remove(7);
		originalCollection.Remove(8);

		// Nothing should have changed
		AreEqual(2, thirdsOnly.Count);
		AreEqual(new[] { 6, 9 }, thirdsOnly.ToArray());
		AreEqual(4, filteredCount);
	}

	#endregion
}