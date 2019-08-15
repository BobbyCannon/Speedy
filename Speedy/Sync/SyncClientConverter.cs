#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync object converter for the sync client.
	/// </summary>
	public class SyncClientConverter
	{
		#region Fields

		private readonly SyncObjectConverter[] _converters;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync converter to be used during syncing.
		/// </summary>
		/// <param name="allowSyncExclusions"> Allow sync exclusion during conversion. </param>
		/// <param name="allowUpdateExclusions"> Allow update exclusion during conversion. </param>
		/// <param name="converters"> The converters to process during conversion. </param>
		public SyncClientConverter(bool allowSyncExclusions = true, bool allowUpdateExclusions = true, params SyncObjectConverter[] converters)
		{
			AllowSyncExclusions = allowSyncExclusions;
			AllowUpdateExclusions = allowUpdateExclusions;

			_converters = converters;
		}

		#endregion

		#region Properties

		/// <summary>
		/// If true excluded sync properties will not be updated otherwise all matching properties will be updated.
		/// </summary>
		public bool AllowSyncExclusions { get; set; }

		/// <summary>
		/// If true excluded update properties will not be updated otherwise all matching properties will be updated.
		/// </summary>
		public bool AllowUpdateExclusions { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Process the provided request through the converters.
		/// </summary>
		/// <param name="collection"> The collection to process. </param>
		/// <returns> The request with an updated collection. </returns>
		public IEnumerable<SyncObject> Process(IList<SyncObject> collection)
		{
			return SyncObjectConverter.Convert(collection, AllowSyncExclusions, AllowUpdateExclusions, _converters).Where(x => x != null).ToList();
		}

		/// <summary>
		/// Process the provided sync object through the converters.
		/// </summary>
		/// <param name="value"> The sync object to process. </param>
		/// <returns> The process sync object. </returns>
		public SyncObject Process(SyncObject value)
		{
			return SyncObjectConverter.Convert(value, AllowSyncExclusions, AllowUpdateExclusions, _converters);
		}

		/// <summary>
		/// Process the provided sync issue through the converters.
		/// </summary>
		/// <param name="issue"> The sync issue to process. </param>
		/// <returns> The process sync issue. </returns>
		public SyncIssue Process(SyncIssue issue)
		{
			return SyncObjectConverter.Convert(issue, _converters);
		}

		#endregion
	}
}