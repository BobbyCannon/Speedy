#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.ServiceHosting.Example;

#endregion

namespace Speedy.UnitTests.ServiceHosting.Example
{
	[TestClass]
	public class ServiceOptionsTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void MessageOption()
		{
			var version = TestHelper.Version;
			var options = GetOptions("-m");
			Assert.AreEqual(1, options.UnknownArguments.Count);
			Assert.AreEqual(null, options.UnknownArguments["-m"]);
			Assert.AreEqual(false, options.IsValid);
			Assert.AreEqual($"Service Hosting {version}\r\n\tThe required argument [m] was not found.\r\n", options.BuildIssueInformation());

			options = GetOptions("-m", "test");
			Assert.AreEqual(0, options.UnknownArguments.Count);
			Assert.AreEqual("test", options.Message);
			Assert.AreEqual(true, options.IsValid);
			Assert.AreEqual($"Service Hosting {version}\r\n", options.BuildIssueInformation());
			Assert.AreEqual("-m \"test\"", options.ToServiceString());
			
			options = GetOptions("-m", "\"test\"");
			Assert.AreEqual(0, options.UnknownArguments.Count);
			Assert.AreEqual("\"test\"", options.Message);
			Assert.AreEqual(true, options.IsValid);
			Assert.AreEqual($"Service Hosting {version}\r\n", options.BuildIssueInformation());
			Assert.AreEqual("-m \"\\\"test\\\"\"", options.ToServiceString());
		}

		[TestMethod]
		public void RequiredMessage()
		{
			var version = TestHelper.Version;
			var options = GetOptions();
			Assert.AreEqual(false, options.IsValid);
			Assert.AreEqual($"Service Hosting {version}\r\n\tThe required argument [m] was not found.\r\n", options.BuildIssueInformation());
		}

		[TestMethod]
		public void StartOption()
		{
			var options = GetOptions("-start", "12");
			Assert.AreEqual(0, options.UnknownArguments.Count);
			Assert.AreEqual(12, options.Start);
			Assert.AreEqual(false, options.IsValid);

			options = GetOptions("-start");
			Assert.AreEqual(0, options.UnknownArguments.Count);
			Assert.AreEqual(1, options.Start);
			Assert.AreEqual(false, options.IsValid);
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