#region References

using System;
using Speedy.Extensions;
using Speedy.Serialization;

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
		public SyncDatabaseProvider(Func<DatabaseOptions, ISyncableDatabase> function, DatabaseOptions options = null)
			: base(function, options)
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
			return (T) GetSyncableDatabase(options);
		}

		#endregion
	}

	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncDatabaseProvider : ISyncableDatabaseProvider
	{
		#region Fields

		private readonly Func<DatabaseOptions, ISyncableDatabase> _function;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync database provider using the provided function.
		/// </summary>
		/// <param name="function"> The function to return the syncable database. </param>
		/// <param name="options"> The options for this database provider. </param>
		public SyncDatabaseProvider(Func<DatabaseOptions, ISyncableDatabase> function, DatabaseOptions options = null)
		{
			_function = function;

			Options = options?.DeepClone() ?? new DatabaseOptions();
		}

		#endregion

		#region Properties

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
			return GetSyncableDatabase(options);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase()
		{
			return _function(Options.DeepClone());
		}

		/// <inheritdoc />
		public ISyncableDatabase GetSyncableDatabase(DatabaseOptions options)
		{
			return _function(options);
		}

		#endregion
	}
}