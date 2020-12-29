#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class BindableTests
	{
		#region Methods

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
			actual.Enabled = false;
			Assert.AreEqual(2, count);

			// Should restore notifications
			actual.PausePropertyChangeNotifications(false);
			actual.Enabled = true;
			Assert.AreEqual(3, count);
		}

		#endregion

		#region Classes

		public class TestBindable : Bindable
		{
			#region Properties

			public bool Enabled { get; set; }

			#endregion
		}

		#endregion
	}
}