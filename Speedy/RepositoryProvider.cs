#region References

using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Speedy
{
	/// <summary>
	/// A provider for the Repository.
	/// </summary>
	public class RepositoryProvider : IRepositoryProvider
	{
		#region Fields

		private readonly DirectoryInfo _directory;

		#endregion

		#region Constructors

		public RepositoryProvider(string directory)
			: this(new DirectoryInfo(directory))
		{
		}

		public RepositoryProvider(DirectoryInfo directory)
		{
			_directory = directory;
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
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <returns> The repository. </returns>
		public IRepository GetRepository(string name)
		{
			return new Repository(_directory.FullName, name);
		}

		#endregion
	}
}