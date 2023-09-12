#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.Data.Client;

/// <summary>
/// Represents the client private address model.
/// </summary>
public class ClientAddress : Address, IClientEntity
{
	#region Constructors

	[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
	public ClientAddress()
	{
		Accounts = new List<ClientAccount>();
		ResetHasChanges();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The people associated with this address.
	/// </summary>
	public virtual ICollection<ClientAccount> Accounts { get; set; }

	/// <summary>
	/// Represents the last time this account was update on the client
	/// </summary>
	public DateTime LastClientUpdate { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override HashSet<string> GetDefaultExclusionsForIncomingSync()
	{
		return base.GetDefaultExclusionsForIncomingSync()
			.AddRange(nameof(Accounts), nameof(LastClientUpdate));
	}

	/// <inheritdoc />
	protected override HashSet<string> GetDefaultExclusionsForOutgoingSync()
	{
		// Update defaults are the same as incoming sync defaults
		var response = base.GetDefaultExclusionsForOutgoingSync()
			.AddRange(GetDefaultExclusionsForIncomingSync());

		return response;
	}

	/// <inheritdoc />
	protected override HashSet<string> GetDefaultExclusionsForSyncUpdate()
	{
		// Update defaults are the same as incoming sync defaults plus some
		return base.GetDefaultExclusionsForSyncUpdate()
			.AddRange(GetDefaultExclusionsForIncomingSync());
	}

	#endregion
}