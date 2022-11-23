#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data;

#endregion

namespace Speedy.UnitTests.Devices;

[TestClass]
public class DeviceManagerTests
{
	#region Methods

	[TestMethod]
	public void name()
	{
		var manager = new TestInformationManager();
	}

	#endregion

	#region Classes

	public class TestDeviceInformation : Speedy.Data.Location.Location
	{
	}

	public class TestInformationManager : InformationManager<TestDeviceInformation>
	{
		#region Constructors

		public TestInformationManager() : base(new DefaultDispatcher())
		{
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public override string ProviderName => "Test Device Information Manager";

		#endregion
	}

	#endregion
}