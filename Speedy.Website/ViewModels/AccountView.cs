#region References

using System;
using System.Collections.Generic;
using Speedy.Data.WebApi;

#endregion

namespace Speedy.Website.ViewModels
{
	public class AccountView : Account
	{
		#region Properties

		public DateTime LastLoginDate { get; set; }

		public string MemberFor { get; set; }

		#endregion
	}
}