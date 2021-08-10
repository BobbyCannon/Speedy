#region References

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Speedy.Data.Updates;
using Speedy.Data.WebApi;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.WebApi
{
	public class AccountController : BaseController
	{
		#region Constructors

		public AccountController(IContosoDatabase database) : base(database)
		{
		}

		#endregion

		#region Methods

		[HttpPut]
		[Route("api/Account")]
		public Account UpdateAccount([FromBody] AccountUpdate update)
		{
			update.Validate();
			var entity = Database.Accounts.FirstOrDefault(x => x.Id == update.Id);
			update.Apply(entity);
			Database.SaveChanges();
			return ToModel(entity);
		}

		private Account ToModel(AccountEntity entity)
		{
			var response = new Account();
			response.UpdateWith(entity);
			return response;
		}

		#endregion
	}
}