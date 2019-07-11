#region References

using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseController : ControllerBase
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

		[HttpGet("Hello")]
		public string Hello()
		{
			return "Hello";
		}

		#endregion
	}
}