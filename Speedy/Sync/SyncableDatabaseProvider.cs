#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncableDatabaseProvider<T> : SyncableDatabaseProvider, ISyncableDatabaseProvider<T>
		where T : ISyncableDatabase
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync database provider using the provided function.
		/// </summary>
		/// <param name="function"> The function to return the syncable database. </param>
		/// <param name="options"> The options for this database provider. </param>
		/// <param name="keyCache"> An optional key manager for tracking entity IDs (primary and sync). </param>
		/// <param name="dispatcher"> An optional dispatcher to update with. </param>
		public SyncableDatabaseProvider(Func<DatabaseOptions, DatabaseKeyCache, ISyncableDatabase> function, DatabaseOptions options, DatabaseKeyCache keyCache, IDispatcher dispatcher = null)
			: base(function, options, keyCache, dispatcher)
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public void BulkProcess(int total, int iterationSize, Action<int, T> process)
		{
			DatabaseProvider<T>.BulkProcess(GetDatabase, total, iterationSize, process);
		}

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		public new T GetDatabase()
		{
			return (T) base.GetSyncableDatabase();
		}

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		public new T GetDatabase(DatabaseOptions options)
		{
			return (T) base.GetSyncableDatabase(options, KeyCache);
		}

		/// <inheritdoc />
		public new T GetSyncableDatabase()
		{
			return (T) base.GetSyncableDatabase();
		}

		/// <inheritdoc />
		public new T GetSyncableDatabase(DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			return (T) base.GetSyncableDatabase(options, keyCache);
		}

		/// <inheritdoc />
		T IDatabaseProvider<T>.GetDatabase()
		{
			return GetSyncableDatabase();
		}

		/// <inheritdoc />
		T IDatabaseProvider<T>.GetDatabase(DatabaseOptions options)
		{
			return GetSyncableDatabase(options, KeyCache);
		}

		#endregion
	}

	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncableDatabaseProvider : Bindable, ISyncableDatabaseProvider
	{
		#region Fields

		private readonly Func<DatabaseOptions, DatabaseKeyCache, ISyncableDatabase> _function;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync database provider using the provided function.
		/// </summary>
		/// <param name="function"> The function to return the syncable database. </param>
		/// <param name="options"> The options for this database provider. </param>
		/// <param name="keyCache"> An optional key manager for managing entity IDs (primary and sync). </param>
		/// <param name="dispatcher"> An optional dispatcher to update with. </param>
		public SyncableDatabaseProvider(Func<DatabaseOptions, DatabaseKeyCache, ISyncableDatabase> function, DatabaseOptions options, DatabaseKeyCache keyCache, IDispatcher dispatcher = null) : base(dispatcher)
		{
			_function = function;

			Options = (DatabaseOptions) options?.DeepClone() ?? new DatabaseOptions();
			KeyCache = keyCache;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DatabaseKeyCache KeyCache { get; set; }

		/// <inheritdoc />
		public DatabaseOptions Options { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public IDatabase GetDatabase()
		{
			return GetSyncableDatabase();
		}

		/// <inheritdoc />
		public IDatabase GetDatabase(DatabaseOptions options)
		{
			return GetSyncableDatabase(options, KeyCache);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase()
		{
			return GetSyncableDatabaseFromProvider((DatabaseOptions) Options.DeepClone(), KeyCache);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase(DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			return GetSyncableDatabaseFromProvider(options, keyCache);
		}

		/// <summary>
		/// Gets an instance of the database from the provider.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <param name="keyCache"> An optional key manager for tracking entity IDs (primary and sync). </param>
		/// <returns> The database instance. </returns>
		protected virtual ISyncableDatabase GetSyncableDatabaseFromProvider(DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			return _function.Invoke(options, keyCache);
		}

		#endregion
	}
}