#region References

using System.Web.Optimization;

#endregion

namespace Speedy.Website
{
	public static class BundleConfig
	{
		#region Methods

		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new StyleBundle("~/Styles/css")
				.Include("~/Content/font-awesome.css")
				.Include("~/Content/speedy.css"));

			bundles.Add(new ScriptBundle("~/Scripts/js")
				.Include("~/Scripts/angular.js")
				.Include("~/Scripts/jquery-{version}.js")
				.Include("~/Scripts/speedy.js"));
		}

		#endregion
	}
}