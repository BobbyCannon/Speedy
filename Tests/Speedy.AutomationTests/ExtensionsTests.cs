#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation;
using Speedy.Automation.Web;

#endregion

namespace Speedy.AutomationTests
{
	[TestClass]
	public class ExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void CountShouldReturnCorrectNumberOfBrowserTypes()
		{
			Assert.AreEqual(3, BrowserType.All.Count());
			Assert.AreEqual(3, (BrowserType.Chrome | BrowserType.Edge | BrowserType.Firefox).Count());
			Assert.AreEqual(2, (BrowserType.Chrome | BrowserType.Edge).Count());
			Assert.AreEqual(2, (BrowserType.Edge | BrowserType.Firefox).Count());
			Assert.AreEqual(2, (BrowserType.Chrome | BrowserType.Firefox).Count());
			Assert.AreEqual(1, BrowserType.Chrome.Count());
			Assert.AreEqual(1, BrowserType.Edge.Count());
			Assert.AreEqual(1, BrowserType.Firefox.Count());
		}

		#endregion
	}
}