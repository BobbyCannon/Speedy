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
			ValidateProperty(x => x.Name)
				.HasMinMaxRange(1, 5)
				.IsNotNull()
				.IsRequired();
		}

		#endregion

		#region Properties

		public int Id
		{
			get => Get<int>(nameof(Id), default);
			set => Set(nameof(Id), value);
		}

		public string Name
		{
			get => Get<string>(nameof(Name), default);
			set => Set(nameof(Name), value);
		}

		public Guid SyncId
		{
			get => Get<Guid>(nameof(SyncId), default);
			set => Set(nameof(SyncId), value);
		}

		#endregion
	}
}