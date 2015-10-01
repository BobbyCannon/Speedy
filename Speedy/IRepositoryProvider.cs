#region References

using System.Collections.Generic;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents provider for a repository of key value pairs.
	/// </summary>
	public interface IRepositoryProvider
	{
		#region Methods

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <returns> A list of repository names that are available to access. </returns>
		IEnumerable<string> AvailableRepositories();

		/// <summary>
		/// Delete a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to delete. </param>
		void DeleteRepository(string name);

		/// <summary>
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <returns> The repository. </returns>
		IRepository OpenRepository(string name);

		#endregion
	}
}