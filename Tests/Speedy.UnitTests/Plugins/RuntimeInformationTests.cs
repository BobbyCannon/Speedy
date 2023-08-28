#if NET6_0_OR_GREATER

#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Application.Wpf;
using Speedy.Automation.Tests;

#endregion

namespace Speedy.UnitTests.Plugins;

[TestClass]
public class RuntimeInformationTests
{
	#region Methods

	[TestMethod]
	public void WindowsRuntimeInformation()
	{
		var runtimeInformation = new WpfRuntimeInformation();
		runtimeInformation.Refresh();
		runtimeInformation.Dump();
	}

	#endregion
}

#endif