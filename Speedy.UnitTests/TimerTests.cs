#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class TimerTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void TimeShouldUseMinimalTime()
		{
			// Warm up the TimeService
			TimeService.Now.Dump();
			TimeService.UtcNow.Dump();
			const int maximumTicks = 700;

			var timer = new Timer();
			timer.Time(() => { });
			timer.Elapsed.Ticks.Dump();
			Assert.IsTrue(timer.Elapsed.Ticks < maximumTicks, $"Timer is taking longer that {maximumTicks} ticks timing nothing...");

			timer.Reset();
			timer.Time(() => { });
			timer.Elapsed.Ticks.Dump();
			Assert.IsTrue(timer.Elapsed.Ticks < maximumTicks, $"Timer is taking longer that {maximumTicks} ticks timing nothing...");

			var timer2 = Timer.Create(() => { });
			timer2.Elapsed.Ticks.Dump();
			Assert.IsTrue(timer2.Elapsed.Ticks < maximumTicks, $"Timer is taking longer that {maximumTicks} ticks timing nothing...");
		}

		#endregion
	}
}