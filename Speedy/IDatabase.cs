#region References

using System;

#endregion

namespace Speedy
{
	/// <summary>
	/// The interfaces for a Speedy database.
	/// </summary>
	public interface IDatabase : IDisposable
	{
		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		DatabaseOptions Options { get; }

		#endregion

		#region Methods

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
		IRepository<T,T2> GetReadOnlyRepository<T,T2>() where T : Entity<T2>, new() where T2 : new();

		/// <summary>
		/// Gets a repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		IRepository<T,T2> GetRepository<T,T2>() where T : Entity<T2>, new() where T2 : new();

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