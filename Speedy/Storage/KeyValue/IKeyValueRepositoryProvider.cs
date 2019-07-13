#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Storage.KeyValue
{
	/// <summary>
	/// Represents provider for a repository of key value pairs.
	/// </summary>
	public interface IKeyValueRepositoryProvider<T>
	{
		#region Methods

		/// <summary>
		/// Archive a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to archive. </param>
		void ArchiveRepository(string name);

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> A list of repository names that are available to access. </returns>
		IEnumerable<string> AvailableRepositories(params string[] excluding);

		/// <summary>
		/// Delete a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to delete. </param>
		void DeleteRepository(string name);

		/// <summary>
		/// Gets the first available repository that is not currently open.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> The repository that was opened or null if none available. </returns>
		IKeyValueRepository<T> OpenAvailableRepository(params string[] excluding);

		/// <summary>
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <param name="options"> The options for the repository. </param>
		/// <returns> The repository. </returns>
		IKeyValueRepository<T> OpenRepository(string name, KeyValueRepositoryOptions options = null);

		/// <summary>
		/// Unarchive a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to unarchive. </param>
		void UnarchiveRepository(string name);

		#endregion
	}
}