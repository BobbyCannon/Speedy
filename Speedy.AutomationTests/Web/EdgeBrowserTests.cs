#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Web;
using Speedy.Automation.Web.Browsers;

#endregion

namespace Speedy.AutomationTests.Web
{
	[TestClass]
	public class EdgeBrowserTests
	{
		#region Methods

		[TestMethod]
		public void CreateBrowser()
		{
			Browser.CloseBrowsers();
			using var browser = Edge.AttachOrCreate();
			browser.NavigateTo("about:blank");
		}

		#endregion
	}
}