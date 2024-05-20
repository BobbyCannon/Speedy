#region References

using System;
using Speedy.Runtime;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

/// <inheritdoc />
public abstract class ClientSyncTest<TSyncType, TSyncManager, TSyncClient, TAccount, T5, TDatabaseProvider>
	: IClientSyncTest<TSyncType, TSyncManager, TSyncClient, TAccount, T5, TDatabaseProvider>
	where TSyncType : Enum
	where TSyncManager : SyncManager<TSyncType>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T5>
	where TDatabaseProvider : SyncableDatabaseProvider
{
	#region Methods

	/// <inheritdoc />
	public abstract TAccount CreateClientSyncAccount();

	/// <inheritdoc />
	public abstract TDatabaseProvider GetClientDatabaseProvider();

	/// <inheritdoc />
	public abstract TSyncManager GetClientSyncManager(TSyncClient serverClient);

	/// <inheritdoc />
	public abstract RuntimeInformation GetRuntimeInformationProvider();

	#endregion
}

/// <summary>
/// The contract for client sync test.
/// </summary>
public interface IClientSyncTest<TSyncType, out TSyncManager, in TSyncClient, out TAccount, T5, out TDatabaseProvider>
	where TSyncType : Enum
	where TSyncManager : SyncManager<TSyncType>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T5>
	where TDatabaseProvider : SyncableDatabaseProvider
{
	#region Methods

	/// <summary>
	/// Create a client sync account.
	/// </summary>
	/// <returns> The account for the client. </returns>
	TAccount CreateClientSyncAccount();

	/// <summary>
	/// Get a client database provider.
	/// </summary>
	/// <returns> The database provider for a client database. </returns>
	TDatabaseProvider GetClientDatabaseProvider();

	/// <summary>
	/// Get a client sync manager.
	/// </summary>
	/// <param name="serverClient"> The client for the server side. </param>
	/// <returns> The sync manager for the client. </returns>
	TSyncManager GetClientSyncManager(TSyncClient serverClient);

	/// <summary>
	/// Get the runtime information provider for the client.
	/// </summary>
	/// <returns> The runtime information provider. </returns>
	RuntimeInformation GetRuntimeInformationProvider();

	#endregion
}