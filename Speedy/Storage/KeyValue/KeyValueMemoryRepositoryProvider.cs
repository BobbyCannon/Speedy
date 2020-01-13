#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Speedy.Exceptions;

#endregion

namespace Speedy.Storage.KeyValue
{
	/// <summary>
	/// A provider for the memory / file repository.
	/// </summary>
	public class KeyValueMemoryRepositoryProvider : KeyValueMemoryRepositoryProvider<string>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository provider class.
		/// </summary>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public KeyValueMemoryRepositoryProvider(TimeSpan? timeout = null, int limit = 0)
			: base(timeout, limit)
		{
		}

		#endregion
	}

	/// <summary>
	/// A provider for the memory / file repository.
	/// </summary>
	public class KeyValueMemoryRepositoryProvider<T> : IKeyValueRepositoryProvider<T>
	{
		#region Fields

		private readonly Dictionary<string, KeyValueMemoryRepository<T>> _archived;
		private readonly Dictionary<string, KeyValueMemoryRepository<T>> _repositories;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository provider class.
		/// </summary>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public KeyValueMemoryRepositoryProvider(TimeSpan? timeout = null, int limit = 0)
		{
			_repositories = new Dictionary<string, KeyValueMemoryRepository<T>>();
			_archived = new Dictionary<string, KeyValueMemoryRepository<T>>();

			Timeout = timeout;
			Limit = limit;
		}

		#endregion

		#region Properties

		/// <summary>
		/// A list of the repositories that have been archived.
		/// </summary>
		public IReadOnlyList<KeyValueMemoryRepository<T>> ArchivedRepositories => new ReadOnlyCollection<KeyValueMemoryRepository<T>>(_archived.Values.ToList());

		/// <summary>
		/// Gets the maximum limit of items to be cached in memory.
		/// </summary>
		public int Limit { get; }

		/// <summary>
		/// A list of the repositories that have been opened.
		/// </summary>
		public IReadOnlyList<KeyValueMemoryRepository<T>> OpenedRepositories => new ReadOnlyCollection<KeyValueMemoryRepository<T>>(_repositories.Values.ToList());

		/// <summary>
		/// Gets the amount of time to cache items in memory before persisting to disk.
		/// </summary>
		public TimeSpan? Timeout { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Archive a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to archive. </param>
		public void ArchiveRepository(string name)
		{
			if (!_repositories.ContainsKey(name))
			{
				throw new SpeedyException(SpeedyException.RepositoryNotFound);
			}

			if (_archived.ContainsKey(name))
			{
				_archived.Remove(name);
			}

			_archived.Add(name, _repositories[name]);
			_repositories.Remove(name);
		}

		/// <summary>
		/// Gets a list of names for available repositories.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> A list of repository names that are available to access. </returns>
		public IEnumerable<string> AvailableRepositories(params string[] excluding)
		{
			return _repositories.Keys.Except(excluding).ToList();
		}

		/// <summary>
		/// Delete a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to delete. </param>
		public void DeleteRepository(string name)
		{
			if (_repositories.ContainsKey(name))
			{
				_repositories.Remove(name);
			}
		}

		/// <summary>
		/// Gets the first available repository that is not currently open.
		/// </summary>
		/// <param name="excluding"> The optional repositories to exclude. </param>
		/// <returns> The repository that was opened or null if none available. </returns>
		public IKeyValueRepository<T> OpenAvailableRepository(params string[] excluding)
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
		/// <param name="options"> The options for the repository. </param>
		/// <returns> The repository. </returns>
		public IKeyValueRepository<T> OpenRepository(string name, KeyValueRepositoryOptions options = null)
		{
			if (options == null)
			{
				options = new KeyValueRepositoryOptions
				{
					Limit = Limit,
					Timeout = Timeout ?? TimeSpan.Zero
				};
			}

			var repository = new KeyValueMemoryRepository<T>(name, options);
			repository.Deleted += RepositoryOnDeleted;

			_repositories.AddOrUpdate(name, repository);

			return repository;
		}

		/// <summary>
		/// Unarchive a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to unarchive. </param>
		public void UnarchiveRepository(string name)
		{
			if (_repositories.ContainsKey(name))
			{
				throw new SpeedyException(SpeedyException.RepositoryNotFound);
			}

			if (!_archived.ContainsKey(name))
			{
				throw new SpeedyException(SpeedyException.RepositoryNotFound);
			}

			_repositories.Add(name, _archived[name]);
			_archived.Remove(name);
		}

		private void RepositoryOnDeleted(object sender, EventArgs e)
		{
			var repository = (KeyValueMemoryRepository<T>) sender;
			repository.Deleted -= RepositoryOnDeleted;

			if (_repositories.ContainsKey(repository.Name))
			{
				_repositories.Remove(repository.Name);
			}
		}

		#endregion
	}
}