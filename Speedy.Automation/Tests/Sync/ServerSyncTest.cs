#region References

using Speedy.Devices;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

public abstract class ServerSyncTest<TSyncClient, T6, T7, TDatabaseProvider>
	: IServerSyncTest<TSyncClient, T6, T7, TDatabaseProvider>
	where TSyncClient : SyncClient
	where T6 : SyncEntity<T7>
	where TDatabaseProvider : ISyncableDatabaseProvider
{
	#region Methods

	public abstract RuntimeInformation GetRuntimeInformationProvider();

	public abstract TDatabaseProvider GetServerDatabaseProvider();

	public abstract T6 GetServerSyncAccount();

	public abstract TSyncClient GetServerSyncClient();

	#endregion
}

public interface IServerSyncTest<TSyncClient, TAccount, T7, TDatabaseProvider>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T7>
{
	#region Methods

	RuntimeInformation GetRuntimeInformationProvider();

	TDatabaseProvider GetServerDatabaseProvider();

	TAccount GetServerSyncAccount();

	TSyncClient GetServerSyncClient();

	#endregion
}