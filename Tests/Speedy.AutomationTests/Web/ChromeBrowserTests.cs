#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Web;
using Speedy.Automation.Web.Browsers;

#endregion

namespace Speedy.AutomationTests.Web
{
	[TestClass]
	public class ChromeBrowserTests
	{
		#region Methods

		[TestMethod]
		public void CreateBrowser()
		{
			Browser.CloseBrowsers(BrowserType.Chrome);
			using var browser = Chrome.AttachOrCreate();
			browser.NavigateTo("about:blank");
		}

		#endregion
	}
}