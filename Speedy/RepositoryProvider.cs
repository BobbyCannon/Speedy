#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Speedy
{
	/// <summary>
	/// A provider for the memory / file repository.
	/// </summary>
	public class RepositoryProvider : IRepositoryProvider
	{
		#region Fields

		private readonly DirectoryInfo _directory;
		private readonly int _limit;
		private readonly TimeSpan? _timeout;

		#endregion

		#region Constructors

		public RepositoryProvider(string directory, TimeSpan? timeout = null, int limit = 0)
			: this(new DirectoryInfo(directory), timeout, limit)
		{
		}

		public RepositoryProvider(DirectoryInfo directory, TimeSpan? timeout = null, int limit = 0)
		{
			_directory = directory;
			_timeout = timeout;
			_limit = limit;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <returns> A list of repository names that are available to access. </returns>
		public IEnumerable<string> AvailableRepositories()
		{
			return _directory
				.GetFiles("*.speedy", SearchOption.TopDirectoryOnly)
				.Select(x => x.Name.Replace(".speedy", string.Empty))
				.ToList();
		}

		/// <summary>
		/// Delete a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to delete. </param>
		public void DeleteRepository(string name)
		{
			new FileInfo($"{_directory.FullName}\\{name}.speedy").SafeDelete();
		}

		/// <summary>
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <returns> The repository. </returns>
		public IRepository OpenRepository(string name)
		{
			return Repository.Create(_directory.FullName, name, _timeout, _limit);
		}

		#endregion
	}
}