#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class VersionExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void IsDefault()
		{
			var scenarios = new[]
			{
				new Version(),
				new Version(0, 0),
				new Version(0, 0, 0),
				new Version(0, 0, 0, 0),
				Version.Parse("0.0"),
				Version.Parse("0.0.0"),
				Version.Parse("0.0.0.0")
			};

			for (var index = 0; index < scenarios.Length; index++)
			{
				index.Dump();
				var scenario = scenarios[index];
				Assert.IsTrue(scenario.IsDefault());
			}
		}

		#endregion
	}
}