#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represents options to be used during a sync.
/// </summary>
public class SyncOptions : CloneableBindable<SyncOptions>
{
	#region Constants

	/// <summary>
	/// The sync key value. This will be included in the default sync options values.
	/// </summary>
	public const string SyncKey = "SyncKey";

	#endregion

	#region Fields

	private readonly Dictionary<string, SyncRepositoryFilter> _filters;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the class.
	/// </summary>
	public SyncOptions() : this(null)
	{
	}

	/// <summary>
	/// Instantiates an instance of the class.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public SyncOptions(IDispatcher dispatcher) : base(dispatcher)
	{
		LastSyncedOnClient = DateTime.MinValue;
		LastSyncedOnServer = DateTime.MinValue;
		ItemsPerSyncRequest = 600;
		SyncDirection = SyncDirection.PullDownThenPushUp;
		Values = new Dictionary<string, string>();

		_filters = new Dictionary<string, SyncRepositoryFilter>();
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
	/// The direction to sync.
	/// </summary>
	public SyncDirection SyncDirection { get; set; }

	/// <summary>
	/// Additional values for synchronizing.
	/// </summary>
	public Dictionary<string, string> Values { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Adds a syncable filter to the options.
	/// </summary>
	public void AddSyncableFilter<T>(Expression<Func<T, bool>> outgoingFilter = null,
		Expression<Func<T, bool>> incomingFilter = null,
		Func<T, Expression<Func<T, bool>>> lookupFilter = null,
		bool skipDeletedItemsOnInitialSync = true)
	{
		AddSyncableFilter(new SyncRepositoryFilter<T>(outgoingFilter, incomingFilter, lookupFilter, skipDeletedItemsOnInitialSync));
	}

	/// <summary>
	/// Adds a syncable filter to the options.
	/// </summary>
	/// <param name="filter"> The syncable filter to be added. </param>
	public void AddSyncableFilter(SyncRepositoryFilter filter)
	{
		if (_filters.ContainsKey(filter.RepositoryType))
		{
			// Update an existing filter
			_filters[filter.RepositoryType] = filter;
			return;
		}

		// Add a new filter.
		_filters.Add(filter.RepositoryType, filter);
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
	/// Load client details from options that are not trusted.
	/// </summary>
	/// <param name="untrustedOptions"> The untrusted options. </param>
	public void LoadClientDetails(SyncOptions untrustedOptions)
	{
		untrustedOptions.Values.AddOrUpdate(SyncDeviceExtensions.ApplicationNameValueKey, untrustedOptions.Values);
		untrustedOptions.Values.AddOrUpdate(SyncDeviceExtensions.ApplicationVersionValueKey, untrustedOptions.Values);
		untrustedOptions.Values.AddOrUpdate(SyncDeviceExtensions.DeviceIdValueKey, untrustedOptions.Values);
		untrustedOptions.Values.AddOrUpdate(SyncDeviceExtensions.DevicePlatformValueKey, untrustedOptions.Values);
		untrustedOptions.Values.AddOrUpdate(SyncDeviceExtensions.DeviceTypeValueKey, untrustedOptions.Values);
	}

	/// <summary>
	/// Resets the syncable filters
	/// </summary>
	public void ResetFilters()
	{
		_filters.Clear();
	}

	/// <summary>
	/// Check to see if a repository has been excluded from syncing.
	/// </summary>
	/// <param name="type"> The type to check for. </param>
	/// <returns> True if the type is filter or false if otherwise. </returns>
	public bool ShouldExcludeRepository(Type type)
	{
		//
		// If we do not have a filter then consider the repository as excluded.
		//
		return ShouldExcludeRepository(type?.ToAssemblyName());
	}

	/// <summary>
	/// Check to see if a repository has been excluded from syncing.
	/// </summary>
	/// <param name="typeAssemblyName"> The type name to check for. Should be in assembly name format. </param>
	/// <returns> True if the type is filter or false if otherwise. </returns>
	public bool ShouldExcludeRepository(string typeAssemblyName)
	{
		//
		// If we do not have a filter then consider the repository as excluded.
		//
		return (_filters.Count > 0) && !_filters.ContainsKey(typeAssemblyName);
	}

	/// <summary>
	/// Update the SyncStatistics with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public override bool UpdateWith(SyncOptions update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			IncludeIssueDetails = update.IncludeIssueDetails;
			ItemsPerSyncRequest = update.ItemsPerSyncRequest;
			LastSyncedOnClient = update.LastSyncedOnClient;
			LastSyncedOnServer = update.LastSyncedOnServer;
			PermanentDeletions = update.PermanentDeletions;
			SyncDirection = update.SyncDirection;
			Values = update.Values.DeepClone();
		}
		else
		{
			this.IfThen(x => !exclusions.Contains(nameof(IncludeIssueDetails)), x => x.IncludeIssueDetails = update.IncludeIssueDetails);
			this.IfThen(x => !exclusions.Contains(nameof(ItemsPerSyncRequest)), x => x.ItemsPerSyncRequest = update.ItemsPerSyncRequest);
			this.IfThen(x => !exclusions.Contains(nameof(LastSyncedOnClient)), x => x.LastSyncedOnClient = update.LastSyncedOnClient);
			this.IfThen(x => !exclusions.Contains(nameof(LastSyncedOnServer)), x => x.LastSyncedOnServer = update.LastSyncedOnServer);
			this.IfThen(x => !exclusions.Contains(nameof(PermanentDeletions)), x => x.PermanentDeletions = update.PermanentDeletions);
			this.IfThen(x => !exclusions.Contains(nameof(SyncDirection)), x => x.SyncDirection = update.SyncDirection);
			this.IfThen(x => !exclusions.Contains(nameof(Values)), x => x.Values = update.Values.DeepClone());
		}

		foreach (var lookup in update._filters)
		{
			AddSyncableFilter(lookup.Value);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			SyncOptions options => UpdateWith(options, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <summary>
	/// Find a filter for the provided repository.
	/// </summary>
	/// <param name="repository"> The repository to process. </param>
	/// <returns> The filter if found or null otherwise. </returns>
	internal SyncRepositoryFilter GetFilter(ISyncableRepository repository)
	{
		return GetFilter(repository?.TypeName);
	}

	/// <summary>
	/// Find the repository filter and check the entity to see if it should be filtered.
	/// </summary>
	/// <param name="typeAssemblyName"> The type of the entity in assembly format. </param>
	/// <param name="entity"> The entity to be tested. </param>
	/// <returns> True if the sync entity should be filter or false if otherwise. </returns>
	internal bool ShouldFilterIncomingEntity(string typeAssemblyName, ISyncEntity entity)
	{
		var filter = GetFilter(typeAssemblyName);
		if ((filter == null) || !filter.HasIncomingFilter)
		{
			return false;
		}

		// Find the "ShouldFilterEntity" method so we can invoke it
		var methods = filter.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
		var method = methods.First(x => x.Name == nameof(ShouldFilterIncomingEntity));
		return (bool) method.Invoke(filter, new object[] { entity });
	}

	/// <summary>
	/// Find a filter for the provided repository.
	/// </summary>
	/// <param name="typeAssemblyName"> The repository type assembly name to process. </param>
	/// <returns> The filter if found or null otherwise. </returns>
	private SyncRepositoryFilter GetFilter(string typeAssemblyName)
	{
		return _filters.ContainsKey(typeAssemblyName) ? _filters[typeAssemblyName] : null;
	}

	#endregion
}