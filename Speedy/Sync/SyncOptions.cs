#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents options to be used during a sync.
	/// </summary>
	public class SyncOptions : Bindable<SyncOptions>
	{
		#region Constants

		/// <summary>
		/// The sync key value. This will be included in the default sync options values.
		/// </summary>
		public const string SyncKey = "SyncKey";

		/// <summary>
		/// The sync version key value. This will be included in the default sync options values.
		/// </summary>
		public const string SyncVersionKey = "SyncVersionKey";

		#endregion

		#region Fields

		private readonly Dictionary<string, SyncRepositoryFilter> _filterLookup;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public SyncOptions(IDispatcher dispatcher = null) : base(dispatcher)
		{
			LastSyncedOnClient = DateTime.MinValue;
			LastSyncedOnServer = DateTime.MinValue;
			ItemsPerSyncRequest = 300;
			Values = new Dictionary<string, string>
			{
				{ SyncVersionKey, "0.0.0.0" }
			};

			_filterLookup = new Dictionary<string, SyncRepositoryFilter>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Include the detail of the exception in the SyncIssue(s) returned.
		/// </summary>
		public bool IncludeIssueDetails { get; set; }

		/// <summary>
		/// Gets or sets the number of objects to be processed per sync request.
		/// </summary>
		public int ItemsPerSyncRequest { get; set; }

		/// <summary>
		/// Gets or sets the client was last synced on date and time.
		/// </summary>
		public DateTime LastSyncedOnClient { get; set; }

		/// <summary>
		/// Gets or sets the server was last synced on date and time.
		/// </summary>
		public DateTime LastSyncedOnServer { get; set; }

		/// <summary>
		/// If true the sync will actually delete entities marked for deletion. Defaults to false where IsDeleted will be marked "true".
		/// </summary>
		public bool PermanentDeletions { get; set; }

		/// <summary>
		/// Additional values for synchronizing.
		/// </summary>
		public Dictionary<string, string> Values { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds a syncable filter to the options.
		/// </summary>
		/// <param name="filter"> The syncable filter to be added. </param>
		public void AddSyncableFilter(SyncRepositoryFilter filter)
		{
			if (_filterLookup.ContainsKey(filter.RepositoryType))
			{
				_filterLookup[filter.RepositoryType] = filter;
			}
			else
			{
				_filterLookup.Add(filter.RepositoryType, filter);
			}
		}

		/// <summary>
		/// Gets the type of sync these options are for
		/// </summary>
		/// <typeparam name="T"> The sync type enumeration type. </typeparam>
		/// <param name="defaultValue"> The default value to return if the Sync Type value is missing or could not be parsed. </param>
		/// <returns> </returns>
		public T GetSyncType<T>(T defaultValue) where T : struct
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}

			return Values.TryGetValue(SyncKey, out var value)
				// Found it now try to parse the sync type
				? Enum.TryParse<T>(value, true, out var sync) ? sync : defaultValue
				// Failed to find the sync key so just assume all
				: defaultValue;
		}

		/// <summary>
		/// Resets the syncable filters
		/// </summary>
		public void ResetFilters()
		{
			_filterLookup.Clear();
		}

		/// <summary>
		/// Check to see if a repository has been filtered.
		/// </summary>
		/// <param name="type"> The type to check for. </param>
		/// <returns> True if the type is filter or false if otherwise. </returns>
		public bool ShouldFilterRepository(Type type)
		{
			return ShouldFilterRepository(type?.ToAssemblyName());
		}

		/// <summary>
		/// Check to see if a repository has been filtered.
		/// </summary>
		/// <param name="typeAssemblyName"> The type name to check for. Should be in assembly name format. </param>
		/// <returns> True if the type is filter or false if otherwise. </returns>
		public bool ShouldFilterRepository(string typeAssemblyName)
		{
			return _filterLookup.Count > 0 && !_filterLookup.ContainsKey(typeAssemblyName);
		}

		/// <summary>
		/// Find a filter for the provided repository.
		/// </summary>
		/// <param name="repository"> The repository to process. </param>
		/// <returns> The filter if found or null otherwise. </returns>
		internal SyncRepositoryFilter GetRepositoryFilter(ISyncableRepository repository)
		{
			return GetRepositoryFilter(repository?.TypeName);
		}

		/// <summary>
		/// Find the repository filter and check the entity to see if it should be filtered.
		/// </summary>
		/// <param name="typeAssemblyName"> The type of the entity in assembly format. </param>
		/// <param name="entity"> The entity to be tested. </param>
		/// <returns> True if the entity should be filter or false if otherwise. </returns>
		internal bool ShouldFilterEntity(string typeAssemblyName, ISyncEntity entity)
		{
			var filter = GetRepositoryFilter(typeAssemblyName);
			if (filter == null)
			{
				return false;
			}

			// Find the "ShouldFilterEntity" method so we can invoke it
			var methods = filter.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
			var method = methods.First(x => x.Name == nameof(ShouldFilterEntity));
			return (bool) method.Invoke(filter, new object[] { entity });
		}

		/// <summary>
		/// Find a filter for the provided repository.
		/// </summary>
		/// <param name="typeAssemblyName"> The repository type assembly name to process. </param>
		/// <returns> The filter if found or null otherwise. </returns>
		private SyncRepositoryFilter GetRepositoryFilter(string typeAssemblyName)
		{
			return _filterLookup.ContainsKey(typeAssemblyName) ? _filterLookup[typeAssemblyName] : null;
		}

		#endregion
	}
}