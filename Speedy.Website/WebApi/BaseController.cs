#region References

using System.Web.Http;
using Speedy.Samples;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseController : ApiController
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