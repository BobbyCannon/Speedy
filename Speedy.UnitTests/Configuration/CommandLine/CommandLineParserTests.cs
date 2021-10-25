#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Configuration.CommandLine;

#endregion

namespace Speedy.UnitTests.Configuration.CommandLine
{
	[TestClass]
	public class CommandLineParserTests
	{
		#region Methods

		[TestMethod]
		public void RequiredFlagArgumentShouldParseWithoutValue()
		{
			var parser = new CommandLineParser();
			Assert.AreEqual(true, parser.IsValid);

			var argument = new CommandLineArgument { Name = "r", Help = "Required value", IsFlag = true, IsRequired = true };
			parser.Add(argument);
			Assert.AreEqual(false, parser.IsValid);

			// Required Flag: should parse

			parser.Parse("-r");
			Assert.AreEqual(null, parser["r"].Value);
			Assert.AreEqual(true, parser["r"].IsValid);
			Assert.AreEqual(true, parser.IsValid);
			Assert.AreEqual(0, parser.UnknownArguments.Count);
		}

		[TestMethod]
		public void RequiredFlagArgumentShouldParseWithProceedingValue()
		{
			var parser = new CommandLineParser();
			Assert.AreEqual(true, parser.IsValid);

			var argument = new CommandLineArgument { Name = "r", Help = "Required value", IsFlag = true, IsRequired = true };
			parser.Add(argument);
			Assert.AreEqual(false, parser.IsValid);

			parser.Parse("-r", "v");
			Assert.AreEqual(null, parser["r"].Value);
			Assert.AreEqual(true, parser["r"].IsValid);
			Assert.AreEqual(true, parser.IsValid);
			Assert.AreEqual(1, parser.UnknownArguments.Count);
			Assert.AreEqual(null, parser.UnknownArguments["v"]);
		}

		[TestMethod]
		public void RequiredValueArgumentShouldNotParseWithoutValue()
		{
			var parser = new CommandLineParser();
			Assert.AreEqual(true, parser.IsValid);

			var argument = new CommandLineArgument { Name = "r", Help = "Required value", IsRequired = true };
			parser.Add(argument);
			Assert.AreEqual(false, parser.IsValid);

			// Require Value: should not parse

			parser.Parse("-r");
			Assert.AreEqual(null, parser["r"].Value);
			Assert.AreEqual(false, parser.IsValid);
			Assert.AreEqual(false, parser.IsValid);
			Assert.AreEqual(1, parser.UnknownArguments.Count);
			Assert.AreEqual(null, parser.UnknownArguments["-r"]);
		}

		[TestMethod]
		public void RequiredValueArgumentShouldParseWithValue()
		{
			var parser = new CommandLineParser();
			Assert.AreEqual(true, parser.IsValid);

			var argument = new CommandLineArgument { Name = "r", Help = "Required value", IsRequired = true };
			parser.Add(argument);
			Assert.AreEqual(false, parser.IsValid);

			// Required Value: should parse

			parser.Parse("-r", "v");
			Assert.AreEqual("v", parser["r"].Value);
			Assert.AreEqual(true, parser["r"].IsValid);
			Assert.AreEqual(true, parser.IsValid);
			Assert.AreEqual(0, parser.UnknownArguments.Count);
		}

		#endregion
	}
}