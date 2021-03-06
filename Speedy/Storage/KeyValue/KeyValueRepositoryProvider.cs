﻿#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Storage.KeyValue
{
	/// <summary>
	/// A provider for the memory / file repository.
	/// </summary>
	public class KeyValueRepositoryProvider : KeyValueRepositoryProvider<string>
	{
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
		public KeyValueRepositoryProvider(string directory, TimeSpan? timeout = null, int limit = 0)
			: base(directory, timeout, limit)
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
		public KeyValueRepositoryProvider(DirectoryInfo directoryInfo, TimeSpan? timeout = null, int limit = 0)
			: base(directoryInfo, timeout, limit)
		{
		}

		#endregion
	}

	/// <summary>
	/// A provider for the memory / file repository.
	/// </summary>
	public class KeyValueRepositoryProvider<T> : IKeyValueRepositoryProvider<T>
	{
		#region Fields

		private readonly DirectoryInfo _directoryInfo;

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
		public KeyValueRepositoryProvider(string directory, TimeSpan? timeout = null, int limit = 0)
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
		public KeyValueRepositoryProvider(DirectoryInfo directoryInfo, TimeSpan? timeout = null, int limit = 0)
		{
			_directoryInfo = directoryInfo;
			Timeout = timeout;
			Limit = limit;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the directory where the repository will reside.
		/// </summary>
		public string Directory => _directoryInfo.FullName;

		/// <summary>
		/// Gets the maximum limit of items to be cached in memory.
		/// </summary>
		public int Limit { get; }

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
			var from = new FileInfo($"{_directoryInfo.FullName}\\{name}.speedy");
			var to = new FileInfo($"{_directoryInfo.FullName}\\{name}.speedy.archive");
			from.SafeMove(to);
		}

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

			return KeyValueRepository<T>.Create(_directoryInfo.FullName, name, options);
		}

		/// <summary>
		/// Unarchive a repository by the provided name.
		/// </summary>
		/// <param name="name"> The name of the repository to unarchive. </param>
		public void UnarchiveRepository(string name)
		{
			var from = new FileInfo($"{_directoryInfo.FullName}\\{name}.speedy.archive");
			var to = new FileInfo($"{_directoryInfo.FullName}\\{name}.speedy");
			from.SafeMove(to);
		}

		#endregion
	}
}