#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.SyncApi;
using Speedy.Validation;

#endregion

namespace Speedy.UnitTests.Helpers
{
	[TestClass]
	public class ValidationHelpersTests
	{
		#region Methods

		[TestMethod]
		public void AssertionShouldWork()
		{
			var account = new Account();
			var validators = new (Func<Account, bool> validator, string message)[]
			{
				(x => x.Id != 0, "Id"),
				(x => !string.IsNullOrWhiteSpace(x.Name), "Name"),
				(x => !string.IsNullOrWhiteSpace(x.EmailAddress), "EmailAddress")
			};

			//TestHelper.ExpectedException<ArgumentException>(() =>
			//	ValidationHelper.ValidateParameters<ArgumentException, Account>(account, validators),
			//	"Id\r\nName\r\nEmailAddress\r\n");

			//account.Name = "Fred";
			
			//TestHelper.ExpectedException<ArgumentException>(() =>
			//		ValidationHelper.ValidateParameters<ArgumentException, Account>(account, validators),
			//	"Id\r\nEmailAddress\r\n");

			//account.Id = 1;
			
			//TestHelper.ExpectedException<ArgumentException>(() =>
			//		ValidationHelper.ValidateParameters<ArgumentException, Account>(account, validators),
			//	"EmailAddress\r\n");
		}

		#endregion
	}
}