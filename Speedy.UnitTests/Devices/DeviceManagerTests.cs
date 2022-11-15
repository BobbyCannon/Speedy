#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Devices;

#endregion

namespace Speedy.UnitTests.Devices;

[TestClass]
public class DeviceManagerTests
{
	#region Methods

	[TestMethod]
	public void name()
	{
		var manager = new TestDeviceInformationManager();
		
	}

	#endregion

	#region Classes

	public class TestDeviceInformation : Speedy.Devices.Location.Location
	{
	}

	public class TestDeviceInformationManager : DeviceInformationManager<TestDeviceInformation>
	{
		#region Constructors

		public TestDeviceInformationManager() : base(new DefaultDispatcher())
		{
		}

		#endregion
	}

	#endregion
}