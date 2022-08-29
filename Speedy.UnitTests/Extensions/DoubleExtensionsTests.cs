#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class DoubleExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void Decrement()
		{
			Assert.AreEqual(0.0, double.Epsilon.Decrement());
			Assert.AreEqual(-1.2345, 0.0.Decrement(1.2345));
			Assert.AreEqual(1.0345, 1.2345.Decrement(0.2));

			var random = new Random();
			var first = random.NextDouble();
			var increment = random.NextDouble();

			Assert.AreEqual(first - increment, first.Decrement(increment));
			Assert.AreEqual(first, first.Decrement(double.NaN));
			Assert.AreEqual(first, first.Decrement(double.PositiveInfinity));
			Assert.AreEqual(first, first.Decrement(double.NegativeInfinity));
		}

		[TestMethod]
		public void Increment()
		{
			Assert.AreEqual(double.Epsilon, 0.0.Increment());
			Assert.AreEqual(1.2345, 0.0.Increment(1.2345));
			Assert.AreEqual(3.4345, 2.2.Increment(1.2345));

			var random = new Random();
			var first = random.NextDouble();
			var increment = random.NextDouble();

			Assert.AreEqual(first + increment, first.Increment(increment));
			Assert.AreEqual(first, first.Increment(double.NaN));
			Assert.AreEqual(first, first.Increment(double.PositiveInfinity));
			Assert.AreEqual(first, first.Increment(double.NegativeInfinity));

			// Increment negative values
			Assert.AreEqual(double.Epsilon * -1, 0.0.Increment(double.Epsilon * -1.0));
		}

		#endregion
	}
}