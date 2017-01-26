#region References

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class LoggingTests
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

			using (var listener = new LogListener(id1))
			{
				Logger.Instance.Write(id1, "First message from logger 1.");

				using (var listener2 = new LogListener(id2))
				{
					Logger.Instance.Write(id2, "First message from logger 2.");
					Logger.Instance.Write(id2, "Second message from logger 2 (critical).", EventLevel.Critical);

					Assert.AreEqual(2, listener2.Events.Count);
					Assert.AreEqual(2, listener2.Events[0].Payload.Count);
					Assert.AreEqual(id2, listener2.Events[0].Payload[0]);
					Assert.AreEqual("First message from logger 2.", listener2.Events[0].Payload[1]);
					Assert.AreEqual(id2, listener2.Events[1].Payload[0]);
					Assert.AreEqual("Second message from logger 2 (critical).", listener2.Events[1].Payload[1]);
				}

				Logger.Instance.Write(id1, "Second message from logger 1 (critical).", EventLevel.Critical);

				Assert.AreEqual(2, listener.Events.Count);
				Assert.AreEqual(2, listener.Events[0].Payload.Count);
				Assert.AreEqual(id1, listener.Events[0].Payload[0]);
				Assert.AreEqual("First message from logger 1.", listener.Events[0].Payload[1]);
				Assert.AreEqual(id1, listener.Events[1].Payload[0]);
				Assert.AreEqual("Second message from logger 1 (critical).", listener.Events[1].Payload[1]);
			}
		}

		[TestMethod]
		public void TestTraceEventSession()
		{
			// create a real time user mode session
			using (var session = new TraceEventSession("Session"))
			{
				session.StopOnDispose = true;
				var watch = Stopwatch.StartNew();

				// Set up Ctrl-C to stop the session
				Console.CancelKeyPress += (s, args) => session.Stop();

				session.Source.AllEvents += delegate(TraceEvent data)
				{
					if (data.EventName.Contains("65534"))
					{
						return;
					}

					Console.WriteLine($"{watch.Elapsed} : {data.EventName} {data.EventDataLength}");
				};

				session.EnableProvider(Guid.Parse(Logger.LoggerGuid));

				Task.Run(() =>
					{
						Console.WriteLine($"{watch.Elapsed} : start");
						session.Source.Process();
					})
					.ContinueWith(x => { Console.WriteLine($"{watch.Elapsed} : done"); });

				var id = Guid.NewGuid();
				Logger.Instance.Write(id, "First message from logger 1.");
				Logger.Instance.Write(id, "Second message from logger 1 (critical).", EventLevel.Critical);

				session.Stop();
			}
		}

		#endregion
	}
}