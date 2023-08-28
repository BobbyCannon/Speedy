#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.NmeaConsole;

#endregion

namespace Speedy.UnitTests.NmeaConsole
{
	[TestClass]
	public class ServiceOptionsTests
	{
		#region Methods

		[TestMethod]
		public void Defaults()
		{
			var options = GetOptions();
			Assert.AreEqual(4800, options.BaudRate);
		}

		private ServiceOptions GetOptions(params string[] arguments)
		{
			var options = new ServiceOptions();
			options.Initialize(arguments);
			return options;
		}

		#endregion
	}
}