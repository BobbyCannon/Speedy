using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.ServiceHosting;

namespace Speedy.UnitTests.ServiceHosting
{
	[TestClass]
	public class WindowsServiceOptionsTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void ShowHelp()
		{
			var options = GetOptions("-h");
			Assert.AreEqual(true, options.ShowHelp);
		}

		private WindowsServiceOptions GetOptions(params string[] arguments)
		{
			var options = new WindowsServiceOptions(Guid.Parse("5BDF6C2A-24C5-47BA-A650-E73A43FDDB3A"), "Test", "Testing");
			options.Initialize(arguments);
			return options;
		}

		#endregion
	}
}