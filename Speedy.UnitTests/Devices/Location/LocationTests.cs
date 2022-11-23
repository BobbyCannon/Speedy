#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.UnitTests.Devices.Location;

[TestClass]
public class LocationTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void CloneShouldWork()
	{
		var expected = new Speedy.Data.Location.Location();
		expected.HorizontalLocation.UpdateWithNonDefaultValues();
		expected.VerticalLocation.UpdateWithNonDefaultValues();
		expected.ToJson(true).Dump();

		var actual = expected.ShallowClone();
		AreEqual(expected, actual);
	}

	[TestMethod]
	public void UpdateWithShouldWork()
	{
		var expected = new Speedy.Data.Location.Location();
		expected.HorizontalLocation.UpdateWithNonDefaultValues();
		expected.VerticalLocation.UpdateWithNonDefaultValues();
		expected.ToJson(true).Dump();

		var actual = new Speedy.Data.Location.Location();
		actual.UpdateWith(expected);
		AreEqual(expected, actual);
	}

	#endregion
}