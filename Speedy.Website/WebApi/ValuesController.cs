#region References

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;

#endregion

namespace Speedy.Website.WebApi
{
	[ApiController]
	[Route("api/[controller]")]
	public class ValuesController : BaseController
	{
		#region Constructors

		public ValuesController(IContosoDatabase database) : base(database)
		{
		}

		#endregion

		#region Methods

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			return new[] { "value1", "value2" };
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		#endregion
	}
}