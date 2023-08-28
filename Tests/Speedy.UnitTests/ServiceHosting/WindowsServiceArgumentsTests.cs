#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.ServiceHosting;

#endregion

namespace Speedy.UnitTests.ServiceHosting
{
	[TestClass]
	public class WindowsServiceArgumentsTests
	{
		#region Methods

		[TestMethod]
		public void GetHelpOutput()
		{
			var options = GetWindowsServiceOptions();
			var actual = options.BuildHelpInformation();
			actual.Escape().Dump();
			Assert.AreEqual($"Foo Bar {TestHelper.Version}\r\n\t[-i] Install the service.\r\n\t[-u] Uninstall the service.\r\n\t[-d] Wait for the debugger.\r\n\t[-v] Verbose logging.\r\n\t[-h] Print out the help menu.\r\n", actual);
		}

		[TestMethod]
		public void IncludeInServiceArgumentsReturnTrueIfRequired()
		{
			var argument = new WindowsServiceArgument
			{
				IncludeInServiceArguments = false,
				IsRequired = false
			};

			Assert.AreEqual(false, argument.IncludeInServiceArguments);

			argument.IsRequired = true;

			Assert.AreEqual(true, argument.IncludeInServiceArguments);
		}

		[TestMethod]
		public void ParseDebugger()
		{
			var options = GetWindowsServiceOptions("-d");
			Assert.AreEqual(true, options.WaitForDebugger);
			Assert.AreEqual("-d", options.ToServiceString());
			Assert.AreEqual("-d", options.ToString());
		}

		[TestMethod]
		public void ParseHelpArgument()
		{
			var options = GetWindowsServiceOptions("-h");
			Assert.AreEqual(true, options.ShowHelp);
			Assert.AreEqual("", options.ToServiceString());
			Assert.AreEqual("-h", options.ToString());
		}

		[TestMethod]
		public void ParseInstallArgument()
		{
			var options = GetWindowsServiceOptions("-i");
			Assert.AreEqual(true, options.InstallService);
			Assert.AreEqual("", options.ToServiceString());
			Assert.AreEqual("-i", options.ToString());
		}

		[TestMethod]
		public void ParseKnownArguments()
		{
			var options = GetWindowsServiceOptions("-i", "-u", "-h", "-d", "-v");
			Assert.AreEqual("-d -v", options.ToServiceString());
			Assert.AreEqual("-i -u -d -v -h", options.ToString());
		}

		[TestMethod]
		public void ParseUninstallArgument()
		{
			var options = GetWindowsServiceOptions("-u");
			Assert.AreEqual(true, options.UninstallService);
			Assert.AreEqual("", options.ToServiceString());
			Assert.AreEqual("-u", options.ToString());
		}

		[TestMethod]
		public void ParseUnknownArgument()
		{
			var options = GetWindowsServiceOptions("-c");
			Assert.AreEqual(1, options.UnknownArguments.Count);
			Assert.AreEqual(null, options.UnknownArguments["-c"]);
			Assert.AreEqual("-c", options.ToServiceString());
			Assert.AreEqual("-c", options.ToString());
		}

		[TestMethod]
		public void ParseUnknownArgumentWithValue()
		{
			var scenarios = new (string[] args, int count, string key, string value, string serviceString, string toString)[]
			{
				(new[] { "-c", "1" }, 1, "-c", "1", "-c 1", "-c 1"),
				(new[] { "-i", "-c", "1" }, 1, "-c", "1", "-c 1", "-i -c 1"),
				(new[] { "-c", "1", "-u" }, 1, "-c", "1", "-c 1", "-u -c 1"),
				(new[] { "-name", "\"Bobby\"", "-u" }, 1, "-name", "\"Bobby\"", "-name \"Bobby\"", "-u -name \"Bobby\"")
			};

			foreach (var scenario in scenarios)
			{
				var options = GetWindowsServiceOptions(scenario.args);
				Assert.AreEqual(scenario.count, options.UnknownArguments.Count);
				Assert.AreEqual(scenario.value, options.UnknownArguments[scenario.key]);
				Assert.AreEqual(scenario.serviceString, options.ToServiceString());
				Assert.AreEqual(scenario.toString, options.ToString());
			}
		}

		[TestMethod]
		public void ParseVerboseArgument()
		{
			var options = GetWindowsServiceOptions("-v");
			Assert.AreEqual(true, options.VerboseLogging);
			Assert.AreEqual("-v", options.ToServiceString());
			Assert.AreEqual("-v", options.ToString());
		}

		private WindowsServiceOptions GetWindowsServiceOptions(params string[] arguments)
		{
			var options = new WindowsServiceOptions(Guid.Parse("29E097C1-5FA4-484B-8442-7ECD1A7D39CA"), "Test", "Foo Bar");
			options.Initialize(arguments);
			return options;
		}

		#endregion
	}
}