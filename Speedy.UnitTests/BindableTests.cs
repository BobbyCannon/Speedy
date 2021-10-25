#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class BindableTests : BaseModelTests<BindableTests.TestBindable>
	{
		#region Methods

		[TestMethod]
		public void AllPropertiesSet()
		{
			ValidateModel(GetModelWithNonDefaultValues());
		}

		[TestMethod]
		public void GetAndSetDispatcher()
		{
			var model = GetModelWithNonDefaultValues();
			Assert.AreEqual(null, model.GetDispatcher());

			var expected = new DefaultDispatcher();
			model.UpdateDispatcher(expected);
			Assert.AreEqual(expected, model.GetDispatcher());
		}

		[TestMethod]
		public void PauseShouldStopNotifications()
		{
			var count = 0;
			var actual = new TestBindable();
			actual.PropertyChanged += (_, _) => count++;
			Assert.AreEqual(0, count);

			// Will trigger "HasChanges" also
			actual.Enabled = true;
			Assert.AreEqual(2, count);

			// Should pause notifications
			actual.PausePropertyChangeNotifications();
			Assert.IsTrue(actual.IsChangeNotificationsPaused());
			actual.Enabled = false;
			Assert.AreEqual(2, count);

			// Should restore notifications
			actual.PausePropertyChangeNotifications(false);
			Assert.IsFalse(actual.IsChangeNotificationsPaused());
			actual.Enabled = true;
			Assert.AreEqual(3, count);
		}

		#endregion

		#region Classes

		public class TestBindable : Bindable
		{
			#region Properties

			public long BigId { get; set; }

			public DateTime CreatedOn { get; set; }

			public bool Enabled { get; set; }

			public int Id { get; set; }

			public virtual DateTime ModifiedOn { get; set; }

			public TimeSpan Timeout { get; set; }

			#endregion
		}

		#endregion
	}
}