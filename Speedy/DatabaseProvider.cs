#region References

using System;
using Speedy.Serialization;

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

			Options = options?.DeepClone() ?? new DatabaseOptions();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public DatabaseOptions Options { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public T GetDatabase()
		{
			return _provider(Options.DeepClone());
		}

		/// <inheritdoc />
		public T GetDatabase(DatabaseOptions options)
		{
			return _provider(options);
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

		/// <summary>
		/// Runs a bulk process where the database lifetime is based on the iteration size.
		/// A database will be instantiated and used for the iteration count. When the iteration
		/// count is reach the database will be saved and disposed. A new database will be created
		/// and processing will continue until the total count is reached. Finally the database
		/// will be saved and disposed.
		/// </summary>
		/// <param name="total"></param>
		/// <param name="iterationSize"></param>
		/// <param name="process"></param>
		public void BulkProcess(int total, int iterationSize, Action<int, T> process)
		{
			var database = GetDatabase();

			for (var i = 1; i <= total; i++)
			{
				process(i, database);

				if (i % iterationSize == 0)
				{
					database.SaveChanges();
					database.Dispose();
					database = default;

					if (i < total)
					{
						database = GetDatabase();
					}
				}
			}

			if (database != null)
			{
				database.SaveChanges();
				database.Dispose();
			}
		}

		#endregion
	}
}