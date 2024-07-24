#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class SpeedyQueueTests : BaseCollectionTests
{
	#region Methods

	[TestMethod]
	public void Limit()
	{
		var queue = new SpeedyQueue<int> { Limit = 3 };
		AreEqual(Array.Empty<int>(), queue);

		queue.Enqueue(1);
		AreEqual(new[] { 1 }, queue);

		queue.Enqueue(2);
		AreEqual(new[] { 1, 2 }, queue);

		queue.Enqueue(3);
		AreEqual(new[] { 1, 2, 3 }, queue);

		queue.Enqueue(4);
		AreEqual(new[] { 2, 3, 4 }, queue);

		queue.Enqueue(5);
		AreEqual(new[] { 3, 4, 5 }, queue);

		queue.Enqueue(6);
		AreEqual(new[] { 4, 5, 6 }, queue);
	}

	[TestMethod]
	public void TryDequeue()
	{
		var queue = new SpeedyQueue<int> { Limit = 3 };
		AreEqual(Array.Empty<int>(), queue);

		queue.Enqueue(1);
		AreEqual(new[] { 1 }, queue);

		IsTrue(queue.TryDequeue(out var actual));
		AreEqual(1, actual);
	}

	#endregion
}