#region References

using System.Web.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.Controllers
{
	public abstract class BaseController : Controller
	{
		#region Constructors

		protected BaseController(IContosoDatabase database)
		{
			Database = database;
		}

		#endregion

		#region Properties

		public IContosoDatabase Database { get; }

		#endregion
	}
}