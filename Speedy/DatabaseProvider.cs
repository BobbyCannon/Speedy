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
		/// Instanciate an instance of the database provider.
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

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		public T GetDatabase()
		{
			return _provider(Options.DeepClone());
		}

		#endregion
	}
}