#region References

using System.Web.Http;
using Speedy.Samples;
using Speedy.Samples.EntityFramework;

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

		#region Methods

		/// <summary> Releases the unmanaged resources that are used by the object and, optionally, releases the managed resources. </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; false to release only unmanaged
		/// resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Database.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}