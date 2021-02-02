#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncDatabaseProvider<T> : SyncDatabaseProvider, ISyncableDatabaseProvider<T>
		where T : ISyncableDatabase
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync database provider using the provided function.
		/// </summary>
		/// <param name="function"> The function to return the syncable database. </param>
		/// <param name="options"> The options for this database provider. </param>
		/// <param name="keyCache"> An optional key manager for tracking entity IDs (primary and sync). </param>
		public SyncDatabaseProvider(Func<DatabaseOptions, DatabaseKeyCache, ISyncableDatabase> function, DatabaseOptions options, DatabaseKeyCache keyCache)
			: base(function, options, keyCache)
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		T IDatabaseProvider<T>.GetDatabase()
		{
			return (T) GetSyncableDatabase();
		}

		/// <inheritdoc />
		T IDatabaseProvider<T>.GetDatabase(DatabaseOptions options)
		{
			return (T) GetSyncableDatabase(options, null);
		}

		#endregion
	}

	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncDatabaseProvider : ISyncableDatabaseProvider
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
		public SyncDatabaseProvider(Func<DatabaseOptions, DatabaseKeyCache, ISyncableDatabase> function, DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			_function = function;

			Options = options?.DeepClone() ?? new DatabaseOptions();
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
			return GetSyncableDatabase(options, null);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase()
		{
			return _function(Options.DeepClone(), KeyCache);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase(DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			return _function(options, keyCache);
		}

		#endregion
	}
}