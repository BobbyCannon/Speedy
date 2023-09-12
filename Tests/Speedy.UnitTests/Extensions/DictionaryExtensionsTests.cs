#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class DictionaryExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void GetOrAdd()
	{
		var dictionary = new Dictionary<string, int>();
		AreEqual(3, dictionary.GetOrAdd("Foo", _ => 3));
		AreEqual(3, dictionary.GetOrAdd("Foo", _ => 5));
	}

	#endregion
}