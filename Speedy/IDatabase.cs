#region References

using System;

#endregion

namespace Speedy
{
	public interface IDatabase : IDisposable
	{
		#region Properties

		DatabaseOptions Options { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets a read only repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		IRepository<T> GetReadOnlyRepository<T>() where T : Entity, new();

		/// <summary>
		/// Gets a repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		IRepository<T> GetRepository<T>() where T : Entity, new();

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