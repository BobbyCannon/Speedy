#region References

using System.Diagnostics.CodeAnalysis;
using System.Web.Optimization;

#endregion

namespace Speedy.Website
{
	[ExcludeFromCodeCoverage]
	public static class BundleConfig
	{
		#region Methods

		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new StyleBundle("~/Styles/css")
				.Include("~/Content/font-awesome.css")
				.Include("~/Content/site.css"));

			bundles.Add(new ScriptBundle("~/Scripts/js")
				.Include("~/Scripts/jquery-{version}.js")
				.Include("~/Scripts/vue.js")
				.Include("~/Scripts/site.js"));
		}

		#endregion
	}
}