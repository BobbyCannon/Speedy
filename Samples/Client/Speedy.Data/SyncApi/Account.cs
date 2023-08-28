#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Data.SyncApi;

/// <summary>
/// Represents the public account model.
/// </summary>
public class Account : SyncModel<int>, IComparable<Account>, IComparable
{
	#region Constructors

	[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
	public Account()
	{
		ResetHasChanges();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The sync ID for the account.
	/// </summary>
	public Guid AddressSyncId { get; set; }

	/// <summary>
	/// The email address for the account.
	/// </summary>
	public string EmailAddress { get; set; }

	/// <summary>
	/// The ID of the account.
	/// </summary>
	public override int Id { get; set; }

	/// <summary>
	/// The name of the account.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The list of roles for the account.
	/// </summary>
	public IEnumerable<string> Roles { get; set; }

	#endregion

	#region Methods

	public int CompareTo(Account other)
	{
		if (ReferenceEquals(this, other))
		{
			return 0;
		}
		if (ReferenceEquals(null, other))
		{
			return 1;
		}
		var addressSyncIdComparison = AddressSyncId.CompareTo(other.AddressSyncId);
		if (addressSyncIdComparison != 0)
		{
			return addressSyncIdComparison;
		}
		var emailAddressComparison = string.Compare(EmailAddress, other.EmailAddress, StringComparison.Ordinal);
		if (emailAddressComparison != 0)
		{
			return emailAddressComparison;
		}
		var idComparison = Id.CompareTo(other.Id);
		if (idComparison != 0)
		{
			return idComparison;
		}
		return string.Compare(Name, other.Name, StringComparison.Ordinal);
	}

	public int CompareTo(object obj)
	{
		return CompareTo((Account) obj);
	}

	#endregion
}