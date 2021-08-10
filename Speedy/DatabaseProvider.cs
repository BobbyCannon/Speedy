#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public class DatabaseProvider<T> : IDatabaseProvider<T> where T : IDatabase
	{
		#region Fields

		private readonly Func<DatabaseOptions, T> _provider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the database provider.
		/// </summary>
		/// <param name="provider"> The database provider function. </param>
		/// <param name="options"> The options for this provider. </param>
		public DatabaseProvider(Func<DatabaseOptions, T> provider, DatabaseOptions options = null)
		{
			_provider = provider;

			Options = (DatabaseOptions) options?.DeepClone() ?? new DatabaseOptions();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DatabaseOptions Options { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public void BulkProcess(int total, int iterationSize, Action<int, T> process)
		{
			BulkProcess(GetDatabase, total, iterationSize, process);
		}

		/// <inheritdoc />
		public T GetDatabase()
		{
			return GetDatabaseFromProvider((DatabaseOptions) Options.DeepClone());
		}

		/// <inheritdoc />
		public T GetDatabase(DatabaseOptions options)
		{
			return GetDatabaseFromProvider(options);
		}

		/// <summary>
		/// Gets an instance of the database from the provider.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <returns> The database instance. </returns>
		protected virtual T GetDatabaseFromProvider(DatabaseOptions options)
		{
			return _provider.Invoke(options);
		}

		/// <summary>
		/// Runs a bulk process where the database lifetime is based on the iteration size.
		/// A database will be instantiated and used for the iteration count. When the iteration
		/// count is reach the database will be saved and disposed. A new database will be created
		/// and processing will continue until the total count is reached. Finally the database
		/// will be saved and disposed.
		/// </summary>
		/// <param name="getDatabase"> Function to get the database. </param>
		/// <param name="total"> The total amount of items to process. </param>
		/// <param name="iterationSize"> The iteration size of each process. </param>
		/// <param name="process"> The action to the process. </param>
		internal static void BulkProcess(Func<T> getDatabase, int total, int iterationSize, Action<int, T> process)
		{
			var database = getDatabase();

			try
			{
				for (var i = 1; i <= total; i++)
				{
					process(i, database);

					if (i % iterationSize == 0)
					{
						database.SaveChanges();
						database.Dispose();

						if (i < total)
						{
							database = getDatabase();
						}
					}
				}

				database.SaveChanges();
			}
			finally
			{
				database.Dispose();
			}
		}

		/// <inheritdoc />
		IDatabase IDatabaseProvider.GetDatabase()
		{
			return GetDatabase();
		}

		/// <inheritdoc />
		IDatabase IDatabaseProvider.GetDatabase(DatabaseOptions options)
		{
			return GetDatabase(options);
		}

		#endregion
	}
}