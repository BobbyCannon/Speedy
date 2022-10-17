#region References

using System;
using Speedy.Devices;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

public abstract class ClientSyncTest<TSyncType, TSyncManager, TSyncClient, TAccount, T5, TDatabaseProvider>
	: IClientSyncTest<TSyncType, TSyncManager, TSyncClient, TAccount, T5, TDatabaseProvider>
	where TSyncType : Enum
	where TSyncManager : SyncManager<TSyncType>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T5>
	where TDatabaseProvider : SyncableDatabaseProvider
{
	#region Methods

	public abstract TAccount CreateClientSyncAccount();

	public abstract TDatabaseProvider GetClientDatabaseProvider();

	public abstract TSyncManager GetClientSyncManager(TSyncClient serverClient);

	public abstract RuntimeInformation GetRuntimeInformationProvider();

	#endregion
}

public interface IClientSyncTest<TSyncType, TSyncManager, TSyncClient, TAccount, T5, TDatabaseProvider>
	where TSyncType : Enum
	where TSyncManager : SyncManager<TSyncType>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T5>
	where TDatabaseProvider : SyncableDatabaseProvider
{
	#region Methods

	TAccount CreateClientSyncAccount();

	TDatabaseProvider GetClientDatabaseProvider();

	TSyncManager GetClientSyncManager(TSyncClient serverClient);

	RuntimeInformation GetRuntimeInformationProvider();

	#endregion
}