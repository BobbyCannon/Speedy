﻿#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Application.Wpf;

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