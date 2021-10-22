#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	public abstract class BaseMessageTests : BaseTests
	{
		#region Methods

		protected void ProcessParseScenarios<T>((string sentance, T expected)[] scenarios)
			where T : NmeaMessage, new()
		{
			foreach (var scenario in scenarios)
			{
				scenario.expected.UpdateChecksum();
				scenario.expected.ToString().Dump();

				var actual = new T();
				actual.Parse(scenario.sentance);
				TestHelper.AreEqual(scenario.expected, actual);

				scenario.expected.UpdateChecksum();
				Assert.AreEqual(scenario.expected.ToString(), actual.ToString());
			}
		}

		#endregion
	}
}