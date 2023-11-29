#region References

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Threading;

#endregion

namespace Speedy.UnitTests
{
    [TestClass]
	public class ThreadSafeTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ThreadSafeDoubleDecrement()
		{
			double actual = 100000;
			var tasks = new List<Task>();
			for (var i = 0; i < 100000; i++)
			{
				// The comment will fail
				//tasks.Add(new Task(() => actual--));
				tasks.Add(new Task(() => ThreadSafe.Decrement(ref actual, 1.0)));
			}
			tasks.ForEach(t => t.Start());
			Task.WaitAll(tasks.ToArray());
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public void ThreadSafeDoubleIncrement()
		{
			var expected = 100000;
			double actual = 0;
			var tasks = new List<Task>();
			for (var i = 0; i < expected; i++)
			{
				// The comment will fail
				//tasks.Add(new Task(() => actual++));
				tasks.Add(new Task(() => ThreadSafe.Increment(ref actual, 1.0)));
			}
			tasks.ForEach(t => t.Start());
			Task.WaitAll(tasks.ToArray());
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ThreadSafeFloatDecrement()
		{
			float actual = 100000;
			var tasks = new List<Task>();
			for (var i = 0; i < 100000; i++)
			{
				// The comment will fail
				//tasks.Add(new Task(() => actual--));
				tasks.Add(new Task(() => ThreadSafe.Decrement(ref actual, 1.0f)));
			}
			tasks.ForEach(t => t.Start());
			Task.WaitAll(tasks.ToArray());
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public void ThreadSafeFloatIncrement()
		{
			var expected = 100000;
			float actual = 0;
			var tasks = new List<Task>();
			for (var i = 0; i < expected; i++)
			{
				// The comment will fail
				//tasks.Add(new Task(() => actual++));
				tasks.Add(new Task(() => ThreadSafe.Increment(ref actual, 1.0f)));
			}
			tasks.ForEach(t => t.Start());
			Task.WaitAll(tasks.ToArray());
			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}