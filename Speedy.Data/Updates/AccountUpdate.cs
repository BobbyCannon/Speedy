#region References

using System;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.Data.Updates
{
	public class AccountUpdate : PartialUpdate<Account>
	{
		#region Constructors

		public AccountUpdate()
		{
			Options.Property(x => x.Name)
				.HasMinMaxRange(1, 450)
				.Throws("Name must be between 1 and 450 characters in length.")
				.IsRequired();
		}

		#endregion

		#region Properties

		public long Id
		{
			get => Get<long>(nameof(Account.Id));
			set => Set(nameof(Id), value);
		}

		public string Name
		{
			get => Get<string>(nameof(Account.Name));
			set => Set(nameof(Name), value);
		}

		public Guid SyncId
		{
			get => Get<Guid>(nameof(Account.SyncId));
			set => Set(nameof(SyncId), value);
		}

		#endregion
	}
}