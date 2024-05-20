#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Runtime;

#endregion

namespace Speedy.UnitTests.Plugins;

[TestClass]
public class RuntimeInformationTests
{
	#region Methods

	[TestMethod]
	public void WindowsRuntimeInformation()
	{
		var runtimeInformation = new RuntimeInformation();
		runtimeInformation.Refresh();
		runtimeInformation.Dump();
	}

	#endregion
}