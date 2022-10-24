#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class BindableTests : SpeedyUnitTest<TrackerPathValue>
	{
		#region Methods

		[TestMethod]
		public void AllPropertiesSet()
		{
			ValidateAllValuesAreNotDefault(GetModelWithNonDefaultValues());
		}

		[TestMethod]
		public void GetAndSetDispatcher()
		{
			var model = GetModelWithNonDefaultValues();
			Assert.AreEqual(null, model.GetDispatcher());

			var expected = new DefaultDispatcher();
			model.UpdateDispatcher(expected);
			TestHelper.AreEqual(expected, model.GetDispatcher());
		}

		[TestMethod]
		public void PauseShouldStopNotifications()
		{
			var count = 0;
			var actual = GetModelWithNonDefaultValues();
			actual.PropertyChanged += (_, _) => count++;
			Assert.AreEqual(0, count);

			// Will trigger "HasChanges" also
			actual.Value = "test";
			Assert.AreEqual(1, count);

			// Should pause notifications
			actual.PausePropertyChangeNotifications();
			Assert.IsTrue(actual.IsChangeNotificationsPaused());
			actual.Value = "foo";
			Assert.AreEqual(1, count);

			// Should restore notifications
			actual.PausePropertyChangeNotifications(false);
			Assert.IsFalse(actual.IsChangeNotificationsPaused());
			actual.Value = "bar";
			Assert.AreEqual(2, count);
		}

		#endregion
	}
}