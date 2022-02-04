#region References

using System;
using System.Diagnostics.Tracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Logging;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class LoggingTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void DisplayEventLevelValues()
		{
			Console.WriteLine((int) EventLevel.Critical);
			Console.WriteLine((int) EventLevel.Error);
			Console.WriteLine((int) EventLevel.LogAlways);
			Console.WriteLine((int) EventLevel.Informational);
			Console.WriteLine((int) EventLevel.Warning);
			Console.WriteLine((int) EventLevel.Verbose);
		}

		[TestMethod]
		public void EnsureLoggerSessionId()
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();

			var startTime = new DateTime(2020, 04, 23, 03, 26, 12, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			using (var listener = MemoryLogListener.CreateSession(id1, EventLevel.Informational))
			{
				Logger.Instance.Write(id1, "First message from logger 1.", EventLevel.Informational);

				using (var listener2 = MemoryLogListener.CreateSession(id2, EventLevel.Informational))
				{
					Logger.Instance.Write(id2, "First message from logger 2.", EventLevel.Informational);
					Logger.Instance.Write(id2, "Second message from logger 2 (critical).", EventLevel.Critical);

					Assert.AreEqual(2, listener2.Events.Count);
					Assert.AreEqual(3, listener2.Events[0].Payload.Count);
					Assert.AreEqual(id2, listener2.Events[0].Payload[0]);
					Assert.AreEqual("First message from logger 2.", listener2.Events[0].Payload[1]);
					Assert.AreEqual(new DateTime(2020, 04, 23, 03, 26, 13, DateTimeKind.Utc), listener2.Events[0].Payload[2]);
					Assert.AreEqual($"4/23/2020 3:26:13 AM - {id2} Informational : First message from logger 2.", listener2.Events[0].GetDetailedMessage());
					Assert.AreEqual(id2, listener2.Events[1].Payload[0]);
					Assert.AreEqual("Second message from logger 2 (critical).", listener2.Events[1].Payload[1]);
					Assert.AreEqual(new DateTime(2020, 04, 23, 03, 26, 14, DateTimeKind.Utc), listener2.Events[1].Payload[2]);
					Assert.AreEqual($"4/23/2020 3:26:14 AM - {id2} Critical : Second message from logger 2 (critical).", listener2.Events[1].GetDetailedMessage());
				}

				Logger.Instance.Write(id1, "Second message from logger 1 (critical).", EventLevel.Critical);

				Assert.AreEqual(2, listener.Events.Count);
				Assert.AreEqual(3, listener.Events[0].Payload.Count);
				Assert.AreEqual(id1, listener.Events[0].Payload[0]);
				Assert.AreEqual("First message from logger 1.", listener.Events[0].Payload[1]);
				Assert.AreEqual(new DateTime(2020, 04, 23, 03, 26, 12, DateTimeKind.Utc), listener.Events[0].Payload[2]);
				Assert.AreEqual($"4/23/2020 3:26:12 AM - {id1} Informational : First message from logger 1.", listener.Events[0].GetDetailedMessage());
				Assert.AreEqual(id1, listener.Events[1].Payload[0]);
				Assert.AreEqual("Second message from logger 1 (critical).", listener.Events[1].Payload[1]);
				Assert.AreEqual(new DateTime(2020, 04, 23, 03, 26, 15, DateTimeKind.Utc), listener.Events[1].Payload[2]);
				Assert.AreEqual($"4/23/2020 3:26:15 AM - {id1} Critical : Second message from logger 1 (critical).", listener.Events[1].GetDetailedMessage());
			}
		}

		#endregion
	}
}