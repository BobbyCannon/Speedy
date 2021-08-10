#region References

using System;
using Speedy.Data.WebApi;

#endregion

namespace Speedy.Data.Updates
{
	public class AccountUpdate : PartialUpdate<Account>
	{
		#region Constructors

		public AccountUpdate()
		{
			Options.IncludeProperties(nameof(Account.Name));
			Options.ExcludeProperties(nameof(Id), nameof(SyncId));
			Options.Property(x => x.Name)
				.HasMinMaxRange(1, 450)
				.Throws("Name must be between 1 and 450 characters in length.")
				.IsRequired();
		}

		#endregion

		#region Properties

		public long Id => GetPropertyValue<long>(nameof(Account.Id));

		public string Name => GetPropertyValue<string>(nameof(Account.Name));

		public Guid SyncId => GetPropertyValue<Guid>(nameof(Account.SyncId));

		#endregion
	}
}