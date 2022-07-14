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
			Validate(x => x.Name)
				.HasMinMaxRange(1, 5, "Name must be between 1 and 5 characters in length.")
				.IsNotNull()
				.IsRequired();
		}

		#endregion

		#region Properties

		public long Id
		{
			get => Get<long>(nameof(Id));
			set => Set(nameof(Id), value);
		}

		public Guid SyncId
		{
			get => Get<Guid>(nameof(SyncId));
			set => Set(nameof(SyncId), value);
		}

		#endregion
	}
}