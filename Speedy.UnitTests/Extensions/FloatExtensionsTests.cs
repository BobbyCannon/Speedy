#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class FloatExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void Decrement()
		{
			Assert.AreEqual(0.0f, float.Epsilon.Decrement());
			Assert.AreEqual(-1.2345f, 0.0.Decrement(1.2345f));
			Assert.AreEqual(1.0345f, 1.2345f.Decrement(0.2f));

			var random = new Random();
			var first = (float) random.NextDouble();
			var increment = (float) random.NextDouble();

			Assert.AreEqual(first - increment, first.Decrement(increment));
			Assert.AreEqual(first, first.Decrement(float.NaN));
			Assert.AreEqual(first, first.Decrement(float.PositiveInfinity));
			Assert.AreEqual(first, first.Decrement(float.NegativeInfinity));
		}

		[TestMethod]
		public void Increment()
		{
			Assert.AreEqual(float.Epsilon, 0.0f.Increment());
			Assert.AreEqual(1.2345f, 0.0f.Increment(1.2345f));
			Assert.AreEqual(3.4345002f, 2.2f.Increment(1.2345f));

			var random = new Random();
			var first = (float) random.NextDouble();
			var increment = (float) random.NextDouble();

			Assert.AreEqual(first + increment, first.Increment(increment));
			Assert.AreEqual(first, first.Increment(float.NaN));
			Assert.AreEqual(first, first.Increment(float.PositiveInfinity));
			Assert.AreEqual(first, first.Increment(float.NegativeInfinity));

			// Increment negative values
			Assert.AreEqual(float.Epsilon * -1, 0.0f.Increment(float.Epsilon * -1.0f));
		}

		#endregion
	}
}