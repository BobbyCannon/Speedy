#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;
using Speedy.Website.ViewModels;

#endregion

namespace Speedy.Website.Services
{
	public class ViewService : BaseService
	{
		#region Constructors

		public ViewService(IContosoDatabase database, AccountEntity account) : base(database, account)
		{
		}

		#endregion

		#region Methods

		public AccountView GetAccount()
		{
			var user = Database.Accounts.First(x => x.Id == Account.Id);
			return ToView(user, TimeService.UtcNow);
		}

		#endregion
	}
}