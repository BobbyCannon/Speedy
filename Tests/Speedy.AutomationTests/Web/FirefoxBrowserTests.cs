#region References

using Speedy.Automation.Web.Browsers;

#endregion

namespace Speedy.AutomationTests.Web
{
	//[TestClass]
	public class FirefoxBrowserTests
	{
		#region Methods

		//[TestMethod]
		public void CreateBrowser()
		{
			using var browser = Firefox.AttachOrCreate();
		}

		#endregion
	}
}