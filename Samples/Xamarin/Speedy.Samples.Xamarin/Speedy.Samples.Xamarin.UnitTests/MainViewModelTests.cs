#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.Location;
using Speedy.UnitTests;

#endregion

namespace Speedy.Samples.Xamarin.UnitTests;

[TestClass]
public class MainViewModelTests : SpeedyTest
{
	#region Methods

	[TestMethod]
	public void ProcessLocationShouldUpdate()
	{
		TestHelper.SetTime(new DateTime(2022, 11, 21, 10, 12, 13));

		var dispatcher = new DefaultDispatcher();
		var mainViewModel = new MainViewModel(dispatcher);
		var horizontal = GetModelWithNonDefaultValues<HorizontalLocation>();
		horizontal.StatusTime = DateTime.UtcNow;

		mainViewModel.ProcessLocation(horizontal);
		var actual = mainViewModel.Locations[0];
		AreEqual(horizontal, actual);

		TestHelper.IncrementTime(seconds: 1);

		horizontal.Latitude = 30.12345;
		horizontal.Longitude = -80.654321;
		horizontal.StatusTime = DateTime.UtcNow;

		mainViewModel.ProcessLocation(horizontal);
		actual = mainViewModel.Locations[0];
		AreEqual(horizontal, actual);
	}

	#endregion
}