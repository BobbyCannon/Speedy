#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.UnitTests;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class TaskExtensionsTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void AwaitResultShouldNotTimeout()
		{
			var task = Task.Run(() =>
			{
				Thread.Sleep(1000);
				return true;
			});
			var result = task.AwaitResults(TimeSpan.FromSeconds(2));
			IsTrue(result);
		}

		[TestMethod]
		public void AwaitResultShouldTimeout()
		{
			var task = Task.Run(() =>
			{
				Thread.Sleep(250);
				return true;
			});

			ExpectedException<TimeoutException>(
				() => task.AwaitResults(TimeSpan.FromMilliseconds(100)),
				"The operation has timed out."
			);
		}

		#endregion
	}
}