#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncEngineStateTests
	{
		#region Methods

		[TestMethod]
		public void IsRunningShouldBeCorrect()
		{
			var scenarios = new Dictionary<SyncEngineStatus, bool>
			{
				{ SyncEngineStatus.Stopped, false },
				{ SyncEngineStatus.Starting, true },
				{ SyncEngineStatus.Pulling, true },
				{ SyncEngineStatus.Pushing, true },
				{ SyncEngineStatus.Completed, false },
				{ SyncEngineStatus.Cancelled, false },
				{ SyncEngineStatus.Failed, false }
			};

			var allStatusValues = Enum.GetValues(typeof(SyncEngineStatus)).Cast<SyncEngineStatus>().ToArray();
			var missingStatuses = allStatusValues.Except(scenarios.Keys).ToArray();

			Assert.AreEqual(0, missingStatuses.Length);

			var state = new SyncEngineState();

			foreach (var scenario in scenarios)
			{
				state.Status = scenario.Key;
				Assert.AreEqual(scenario.Value, state.IsRunning, $"State: {state.Status} should be {scenario.Value}.");
			}
		}

		[TestMethod]
		public void SyncEngineStateShouldClone()
		{
			var testItems = new[]
			{
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 1, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = "123", Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Starting, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 1 }
			};

			CloneableHelper.BaseShouldCloneTest(testItems);
		}

		[TestMethod]
		public void ToStringShouldFormat()
		{
			var state = new SyncEngineState();
			Assert.AreEqual("Stopped", state.ToString());

			state.Status = SyncEngineStatus.Starting;
			Assert.AreEqual("Starting", state.ToString());

			state.Total = 100;
			Assert.AreEqual("Starting: 0 of 100 (0%)", state.ToString());

			state.Status = SyncEngineStatus.Pulling;
			state.Count = 3;
			Assert.AreEqual("Pulling: 3 of 100 (3%)", state.ToString());

			state.Status = SyncEngineStatus.Pushing;
			state.Total = 33;
			state.Count = 12;
			Assert.AreEqual("Pushing: 12 of 33 (36.36%)", state.ToString());

			state.Status = SyncEngineStatus.Cancelled;
			Assert.AreEqual("Cancelled: 12 of 33 (36.36%)", state.ToString());

			state.Status = SyncEngineStatus.Stopped;
			Assert.AreEqual("Stopped: 12 of 33 (36.36%)", state.ToString());

			state.Status = SyncEngineStatus.Failed;
			Assert.AreEqual("Failed: 12 of 33 (36.36%)", state.ToString());

			state.Message = "Boom not good...";
			Assert.AreEqual("Failed: 12 of 33 (36.36%) - Boom not good...", state.ToString());

			state.Status = SyncEngineStatus.Failed;
			state.Count = 0;
			state.Total = 0;
			Assert.AreEqual("Failed: Boom not good...", state.ToString());

			state.Message = string.Empty;
			Assert.AreEqual("Failed", state.ToString());
		}

		[TestMethod]
		public void UpdateWith()
		{
			var expected = new SyncEngineState
			{
				Count = 1,
				HasChanges = true,
				Message = "123",
				Status = SyncEngineStatus.Cancelled,
				Total = 2
			};

			var actual = new SyncEngineState();

			// This should not affect the state of the "actual" state.
			actual.UpdateWith((object) null);
			actual.UpdateWith(null);

			Assert.AreEqual(0, actual.Count);
			Assert.AreEqual(false, actual.HasChanges);
			Assert.AreEqual(null, actual.Message);
			Assert.AreEqual(SyncEngineStatus.Stopped, actual.Status);
			Assert.AreEqual(0, actual.Total);

			actual.UpdateWith((object) expected);
			TestHelper.AreEqual(expected, actual);

			expected.Count = 3;
			actual.UpdateWith(expected, nameof(SyncEngineState.HasChanges));
			TestHelper.AreEqual(expected, actual);
		}

		#endregion
	}
}