#region References

using Speedy.Runtime;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

/// <inheritdoc />
public abstract class ServerSyncTest<TSyncClient, T6, T7, TDatabaseProvider>
	: IServerSyncTest<TSyncClient, T6, T7, TDatabaseProvider>
	where TSyncClient : SyncClient
	where T6 : SyncEntity<T7>
	where TDatabaseProvider : ISyncableDatabaseProvider
{
	#region Methods

	/// <inheritdoc />
	public abstract RuntimeInformation GetRuntimeInformationProvider();

	/// <inheritdoc />
	public abstract TDatabaseProvider GetServerDatabaseProvider();

	/// <inheritdoc />
	public abstract T6 GetServerSyncAccount();

	/// <inheritdoc />
	public abstract TSyncClient GetServerSyncClient();

	#endregion
}

/// <summary>
/// The contract for server sync test.
/// </summary>
public interface IServerSyncTest<out TSyncClient, out TAccount, T7, out TDatabaseProvider>
	where TSyncClient : SyncClient
	where TAccount : SyncEntity<T7>
{
	#region Methods

	/// <summary>
	/// Get the runtime information provider for the server.
	/// </summary>
	/// <returns> The runtime information provider. </returns>
	RuntimeInformation GetRuntimeInformationProvider();

	/// <summary>
	/// Get a server database provider.
	/// </summary>
	/// <returns> The database provider for a server database. </returns>
	TDatabaseProvider GetServerDatabaseProvider();

	/// <summary>
	/// Create a server sync account.
	/// </summary>
	/// <returns> The account for the client. </returns>
	TAccount GetServerSyncAccount();

	/// <summary>
	/// Create a server sync client.
	/// </summary>
	/// <returns> The sync client for the server. </returns>
	TSyncClient GetServerSyncClient();

	#endregion
}