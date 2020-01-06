﻿#region References

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
		/// <param name="excludePropertiesForSync"> Allow property exclusion during conversion in sync. </param>
		/// <param name="excludePropertiesForUpdate"> Allow property exclusion during conversion in update. </param>
		/// <param name="converters"> The converters to process during conversion. </param>
		public SyncClientConverter(bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true, params SyncObjectConverter[] converters)
		{
			ExcludePropertiesForSync = excludePropertiesForSync;
			ExcludePropertiesForUpdate = excludePropertiesForUpdate;

			_converters = converters;
		}

		#endregion

		#region Properties

		/// <summary>
		/// If true excluded properties will not be changed during sync.
		/// </summary>
		public bool ExcludePropertiesForSync { get; set; }

		/// <summary>
		/// If true excluded properties will not be changed during update.
		/// </summary>
		public bool ExcludePropertiesForUpdate { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Process the provided request through the converters.
		/// </summary>
		/// <param name="collection"> The collection to process. </param>
		/// <returns> The request with an updated collection. </returns>
		public IEnumerable<SyncObject> Convert(IEnumerable<SyncObject> collection)
		{
			return SyncObjectConverter.Convert(collection, SyncConversionType.Converting, ExcludePropertiesForSync, ExcludePropertiesForUpdate, _converters).Where(x => x != null).ToList();
		}

		/// <summary>
		/// Process the provided sync object through the converters.
		/// </summary>
		/// <param name="value"> The sync object to process. </param>
		/// <returns> The process sync object. </returns>
		public SyncObject Convert(SyncObject value)
		{
			return SyncObjectConverter.Convert(value, SyncConversionType.Converting, ExcludePropertiesForSync, ExcludePropertiesForUpdate, _converters);
		}

		/// <summary>
		/// Process the provided sync issue through the converters.
		/// </summary>
		/// <param name="issue"> The sync issue to process. </param>
		/// <returns> The process sync issue. </returns>
		public SyncIssue Convert(SyncIssue issue)
		{
			return SyncObjectConverter.Convert(issue, _converters);
		}

		/// <summary>
		/// Gets the converter for the sync object.
		/// </summary>
		/// <param name="syncObject"> The incoming sync object. </param>
		/// <returns> The converter for the sync object. </returns>
		public SyncObjectConverter GetConverter(SyncObject syncObject)
		{
			return _converters.FirstOrDefault(x => x.CanConvert(syncObject));
		}

		#endregion
	}
}