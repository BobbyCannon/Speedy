#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;
using Speedy.UnitTests.Protocols.Samples;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscCommunicationHandlerTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void HandlerShouldBeCalled()
		{
			var actual = false;
			Func<object, SampleOscCommand, bool> test = (o, t) =>
			{
				actual = true;
				return actual;
			};

			var expected = new SampleOscCommand();
			var handler = new OscCommandHandler<SampleOscCommand>(test);
			handler.Process(handler, expected.ToMessage());
			Assert.IsTrue(actual, "Handler was not called");
		}

		#endregion
	}
}