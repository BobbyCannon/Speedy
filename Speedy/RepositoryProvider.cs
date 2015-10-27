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

		private readonly DirectoryInfo _directoryInfo;
		private readonly int _limit;
		private readonly TimeSpan? _timeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository provider class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public RepositoryProvider(string directory, TimeSpan? timeout = null, int limit = 0)
			: this(new DirectoryInfo(directory), timeout, limit)
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository provider class.
		/// </summary>
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public RepositoryProvider(DirectoryInfo directoryInfo, TimeSpan? timeout = null, int limit = 0)
		{
			_directoryInfo = directoryInfo;
			_timeout = timeout;
			_limit = limit;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> A list of repository names that are available to access. </returns>
		public IEnumerable<string> AvailableRepositories(params string[] excluding)
		{
			return _directoryInfo
				.GetFiles("*.speedy", SearchOption.TopDirectoryOnly)
				.Select(x => x.Name.Replace(".speedy", string.Empty))
				.Except(excluding)
				.ToList();
		}

		/// <summary>
		/// Delete a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to delete. </param>
		public void DeleteRepository(string name)
		{
			new FileInfo($"{_directoryInfo.FullName}\\{name}.speedy").SafeDelete();
		}

		/// <summary>
		/// Gets the first available repository that is not currently open.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> The repository that was opened or null if none available. </returns>
		public IRepository OpenAvailableRepository(params string[] excluding)
		{
			foreach (var repository in AvailableRepositories(excluding))
			{
				try
				{
					return OpenRepository(repository);
				}
				catch
				{
					// Nothing to do here. Just move on to the next repository.
				}
			}

			return null;
		}

		/// <summary>
		/// Gets a repository by the provided name. If the repository cannot be found a new one is created and returned.
		/// </summary>
		/// <param name="name"> The name of the repository to get. </param>
		/// <returns> The repository. </returns>
		public IRepository OpenRepository(string name)
		{
			return Repository.Create(_directoryInfo.FullName, name, _timeout, _limit);
		}

		#endregion
	}
}