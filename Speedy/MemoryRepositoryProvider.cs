#region References

using System.Collections.Generic;

#endregion

namespace Speedy
{
	/// <summary>
	/// A provider for the MemoryRepository.
	/// </summary>
	public class MemoryRepositoryProvider : IRepositoryProvider
	{
		#region Fields

		private readonly Dictionary<string, MemoryRepository> _repositories;

		#endregion

		#region Constructors

		public MemoryRepositoryProvider()
		{
			_repositories = new Dictionary<string, MemoryRepository>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <returns> A list of repository names that are available to access. </returns>
		public IEnumerable<string> AvailableRepositories()
		{
			return _repositories.Keys;
		}

		/// <summary>
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <returns> The repository. </returns>
		public IRepository GetRepository(string name)
		{
			if (_repositories.ContainsKey(name))
			{
				return _repositories[name];
			}

			var repository = new MemoryRepository(name);
			_repositories.Add(name, repository);
			return repository;
		}

		#endregion
	}
}