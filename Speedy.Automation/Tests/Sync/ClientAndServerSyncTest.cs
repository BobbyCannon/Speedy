#region References

using System;
using System.Linq;
using Speedy.Devices;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Sync;

public abstract class ClientAndServerSyncTest<TSyncType, TSyncManager, TSyncClient,
        TAccount, TAccountKey, TClientDatabaseProvider, TServerDatabaseProvider>
    : ClientAndServerSyncTest<TSyncType, TSyncManager, TSyncClient,
        TAccount, TAccountKey, TAccount, TAccountKey,
        TClientDatabaseProvider, TServerDatabaseProvider>
    where TSyncType : Enum
    where TSyncManager : SyncManager<TSyncType>
    where TSyncClient : SyncClient
    where TAccount : SyncEntity<TAccountKey>
    where TClientDatabaseProvider : SyncableDatabaseProvider
    where TServerDatabaseProvider : SyncableDatabaseProvider
{
}

public abstract class ClientAndServerSyncTest<TSyncType, TSyncManager, TSyncClient,
        TClientAccount, T5, TServerAccount, T7, TClientDatabaseProvider, TServerDatabaseProvider>
    : SpeedyTest,
        IClientSyncTest<TSyncType, TSyncManager, TSyncClient, TClientAccount, T5, TClientDatabaseProvider>,
        IServerSyncTest<TSyncClient, TServerAccount, T7, TServerDatabaseProvider>
    where TSyncType : Enum
    where TSyncManager : SyncManager<TSyncType>
    where TSyncClient : SyncClient
    where TClientAccount : SyncEntity<T5>
    where TServerAccount : SyncEntity<T7>
    where TClientDatabaseProvider : SyncableDatabaseProvider
    where TServerDatabaseProvider : SyncableDatabaseProvider
{
    #region Properties

    public TClientDatabaseProvider ClientDatabaseProvider => GetClientDatabaseProvider();

    #endregion

    #region Methods

    /// <inheritdoc />
    public abstract TClientAccount CreateClientSyncAccount();

    /// <inheritdoc />
    public abstract TClientDatabaseProvider GetClientDatabaseProvider();

    /// <inheritdoc />
    public abstract TSyncManager GetClientSyncManager(TSyncClient serverClient);

    /// <summary>
    /// Get the runtime information provider.
    /// </summary>
    /// <returns> The runtime information. </returns>
    public abstract RuntimeInformation GetRuntimeInformationProvider();

    /// <inheritdoc />
    public abstract TServerDatabaseProvider GetServerDatabaseProvider();

    /// <inheritdoc />
    public abstract TServerAccount GetServerSyncAccount();

    /// <inheritdoc />
    public abstract TSyncClient GetServerSyncClient();

    /// <summary>
    /// Process the sync.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="setupSync"></param>
    /// <param name="updateOptions"></param>
    /// <param name="waitFor"></param>
    /// <param name="postAction"></param>
    /// <returns></returns>
    public SyncResults<TSyncType> Process(TSyncType type,
        Action<TClientDatabaseProvider, TClientAccount, TServerDatabaseProvider, TServerAccount> setupSync,
        Action<SyncOptions> updateOptions = null, TimeSpan? waitFor = null, Action<SyncResults<TSyncType>> postAction = null)
    {
        var clientDatabaseProvider = GetClientDatabaseProvider();
        var clientAccount = CreateClientSyncAccount();
        var serverProvider = GetServerDatabaseProvider();
        var serverAccount = GetServerSyncAccount();

        using (var database = clientDatabaseProvider.GetDatabase())
        {
            var repository = database.GetRepository<TClientAccount, T5>();
            repository.Add(clientAccount);
            database.SaveChanges();
        }

        using (var database = serverProvider.GetDatabase())
        {
            var repository = database.GetRepository<TServerAccount, T7>();
            repository.Add(serverAccount);
            database.SaveChanges();
        }

        // Setup the databases and such before the sync
        setupSync.Invoke(clientDatabaseProvider, clientAccount, serverProvider, serverAccount);

        var serverSyncClient = GetServerSyncClient();
        var clientSyncManager = GetClientSyncManager(serverSyncClient);
        return clientSyncManager.ProcessAsync(type, updateOptions, waitFor, postAction).AwaitResults();
    }

    public void ValidateSyncResults(SyncResults<TSyncType> results)
    {
        AreEqual(0, results.SyncIssues.Count,
            () => string.Join(Environment.NewLine, results.SyncIssues.Select(x => x.Message))
        );
    }

    #endregion
}