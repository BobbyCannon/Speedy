#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync database provider.
	/// </summary>
	public class SyncDatabaseProvider : ISyncableDatabaseProvider
	{
		#region Fields

		private readonly Func<ISyncableDatabase> _function;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync database provider using the provided function.
		/// </summary>
		/// <param name="function"> The function to return the syncable database. </param>
		public SyncDatabaseProvider(Func<ISyncableDatabase> function)
		{
			_function = function;
			Options = new DatabaseOptions();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DatabaseOptions Options { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public ISyncableDatabase GetDatabase()
		{
			return _function();
		}

		/// <inheritdoc />
		IDatabase IDatabaseProvider.GetDatabase()
		{
			return GetDatabase();
		}

		#endregion
	}
}