#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class PathTests
	{
		#region Methods

		[TestMethod]
		public void CompleteShouldWork()
		{
			var currentTime = new DateTime(2020, 12, 15, 01, 02, 03, 999, DateTimeKind.Utc);

			// ReSharper disable once AccessToModifiedClosure
			TimeService.UtcNowProvider = () => currentTime;

			var path = new TrackerPath();
			Assert.AreEqual(false, path.IsCompleted);
			Assert.AreEqual(0, path.ElapsedTime.Ticks);

			currentTime = currentTime.AddMilliseconds(1);

			path.Complete();
			Assert.AreEqual(true, path.IsCompleted);
			Assert.AreEqual(1, path.ElapsedTime.TotalMilliseconds);
		}

		#endregion
	}
}