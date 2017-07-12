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

		private readonly Func<T> _provider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instanciate an instance of the database provider.
		/// </summary>
		/// <param name="provider"> The database provider function. </param>
		public DatabaseProvider(Func<T> provider)
		{
			_provider = provider;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		public T GetDatabase()
		{
			return _provider();
		}

		#endregion
	}
}