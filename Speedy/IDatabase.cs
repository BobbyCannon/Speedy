#region References

using System;
using System.Collections.Specialized;

#endregion

namespace Speedy
{
	/// <summary>
	/// The interfaces for a Speedy database.
	/// </summary>
	public interface IDatabase : INotifyCollectionChanged, IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		DatabaseOptions Options { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds an entity to the database
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <param name="item"> The item to be added. </param>
		/// <returns> The entity that was added. </returns>
		T Add<T, T2>(T item) where T : Entity<T2>;

		/// <summary>
		/// Discard all changes made in this context to the underlying database.
		/// </summary>
		int DiscardChanges();

		/// <summary>
		/// Gets a read only repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		IRepository<T, T2> GetReadOnlyRepository<T, T2>() where T : Entity<T2>;

		/// <summary>
		/// Gets a repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		IRepository<T, T2> GetRepository<T, T2>() where T : Entity<T2>;

		/// <summary>
		/// Removes an entity from the database
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <param name="item"> The item to be removed. </param>
		/// <returns> The entity that was removed. </returns>
		T Remove<T, T2>(T item) where T : Entity<T2>;

		/// <summary>
		/// Saves all changes made in this context to the underlying database.
		/// </summary>
		/// <returns>
		/// The number of objects written to the underlying database.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException"> Thrown if the context has been disposed. </exception>
		int SaveChanges();

		#endregion
	}
}