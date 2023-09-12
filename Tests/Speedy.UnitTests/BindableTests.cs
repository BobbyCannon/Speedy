#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Profiling;
using Speedy.Serialization;

#endregion

namespace Speedy.UnitTests;

[TestClass]
public class BindableTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void AllPropertiesSet()
	{
		var actual = Activator.CreateInstanceWithNonDefaultValues<TrackerPathValue>();
		actual.ValidateAllValuesAreNotDefault();
	}

	[TestMethod]
	public void GetAndSetDispatcher()
	{
		var model = Activator.CreateInstanceWithNonDefaultValues<TrackerPathValue>();
		Assert.AreEqual(null, model.GetDispatcher());

		var expected = new TestDispatcher();
		model.UpdateDispatcher(expected);
		AreEqual(expected, model.GetDispatcher());
	}

	[TestMethod]
	public void HasChangesShouldReset()
	{
		var actual = Activator.CreateInstance<TrackerPathValue>();
		IsFalse(actual.HasChanges);

		actual.Name = "Fred";
		IsTrue(actual.HasChanges);

		actual.GetChangedProperties();
		actual.ResetHasChanges();
	}

	[TestMethod]
	public void DisabledShouldStopNotifications()
	{
		var count = 0;
		var actual = Activator.CreateInstance<TrackerPathValue>();
		actual.PropertyChanged += (_, _) => count++;
		Assert.AreEqual(0, count);

		// Will trigger "HasChanges" also
		actual.Value = "test";
		Assert.AreEqual(1, count);

		// Should pause notifications
		actual.DisablePropertyChangeNotifications();
		Assert.IsFalse(actual.IsPropertyChangeNotificationsEnabled());
		actual.Value = "foo";
		Assert.AreEqual(1, count);

		// Should restore notifications
		actual.EnablePropertyChangeNotifications();
		Assert.IsTrue(actual.IsPropertyChangeNotificationsEnabled());
		actual.Value = "bar";
		Assert.AreEqual(2, count);
	}

	[TestMethod]
	public void SerializationShouldIgnoreBaseProperties()
	{
		var model = Activator.CreateInstance<TrackerPathValue>();
		model.Name = "John Doe";
		model.Value = "Value";

		var actual = model.ToJson();
		var expected = "{\"$id\":\"1\",\"Name\":\"John Doe\",\"Value\":\"Value\"}";

		AreEqual(expected, actual);

		actual = model.ToRawJson();
		expected = "{\"Name\":\"John Doe\",\"Value\":\"Value\"}";

		AreEqual(expected, actual);
	}

	#endregion
}