#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents an object converter.
	/// </summary>
	/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
	/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
	/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
	/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
	public class SyncObjectConverter<T1, T2, T3, T4> : SyncObjectConverter
		where T1 : SyncEntity<T2>
		where T3 : SyncEntity<T4>
	{
		#region Fields

		private readonly Func<T1, T3, SyncConversionType, bool> _convert;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of a converter.
		/// </summary>
		public SyncObjectConverter() : this(null)
		{
		}

		/// <summary>
		/// Instantiates an instance of a converter.
		/// </summary>
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		public SyncObjectConverter(Func<T1, T3, SyncConversionType, bool> convert)
		{
			_convert = convert;

			SourceName = typeof(T1).GetRealType().ToAssemblyName();
			DestinationName = typeof(T3).GetRealType().ToAssemblyName();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanConvert(ISyncEntity syncEntity)
		{
			return syncEntity is T1;
		}

		/// <inheritdoc />
		public override bool CanConvert(SyncObject syncObject)
		{
			return SourceName == syncObject.TypeName;
		}

		/// <inheritdoc />
		public override bool CanConvert(SyncIssue syncIssue)
		{
			return SourceName == syncIssue.TypeName;
		}

		/// <inheritdoc />
		public override SyncObject Convert(SyncObject syncObject, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true)
		{
			return syncObject.Convert<T1, T2, T3, T4>(_convert, type, excludePropertiesForSync, excludePropertiesForUpdate);
		}

		/// <inheritdoc />
		public override void Convert(SyncObject syncObject, ISyncEntity destination, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true)
		{
			var source = syncObject.ToSyncEntity<T1, T2>();
			Convert<T1, T2, T3, T4>(source, (T3) destination, _convert, type, excludePropertiesForSync, excludePropertiesForUpdate);
		}

		/// <inheritdoc />
		public override bool Convert(ISyncEntity source, ISyncEntity destination, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true)
		{
			return Convert<T1, T2, T3, T4>((T1) source, (T3) destination, _convert, type, excludePropertiesForSync, excludePropertiesForUpdate);
		}

		/// <inheritdoc />
		public override SyncIssue Convert(SyncIssue syncIssue)
		{
			return syncIssue.Convert(DestinationName);
		}

		#endregion
	}

	/// <summary>
	/// Represents an object converter.
	/// </summary>
	public abstract class SyncObjectConverter
	{
		#region Properties

		/// <summary>
		/// The destination type name.
		/// </summary>
		protected string DestinationName { get; set; }

		/// <summary>
		/// The source type name.
		/// </summary>
		protected string SourceName { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Test a sync entity to see if this converter can process this object.
		/// </summary>
		/// <param name="syncEntity"> The sync entity to test. </param>
		/// <returns> True if the sync object can be process or false if otherwise. </returns>
		public abstract bool CanConvert(ISyncEntity syncEntity);

		/// <summary>
		/// Test a sync object to see if this converter can process this object.
		/// </summary>
		/// <param name="syncObject"> The sync object to test. </param>
		/// <returns> True if the sync object can be process or false if otherwise. </returns>
		public abstract bool CanConvert(SyncObject syncObject);

		/// <summary>
		/// Test a sync issue to see if this converter can process this object.
		/// </summary>
		/// <param name="syncIssue"> The sync issue to test. </param>
		/// <returns> True if the sync issue can be process or false if otherwise. </returns>
		public abstract bool CanConvert(SyncIssue syncIssue);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <param name="syncObject"> The sync object to process. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public abstract SyncObject Convert(SyncObject syncObject, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <param name="syncObject"> The sync object to process. </param>
		/// <param name="destination"> The destination sync entity to be updated. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public abstract void Convert(SyncObject syncObject, ISyncEntity destination, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <param name="source"> The source sync entity with the updates. </param>
		/// <param name="destination"> The destination sync entity to be updated. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public abstract bool Convert(ISyncEntity source, ISyncEntity destination, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true);

		/// <summary>
		/// Convert this sync issue to a different sync object
		/// </summary>
		/// <param name="syncIssue"> The sync issue to process. </param>
		/// <returns> The converted sync issue in a sync issue format. </returns>
		public abstract SyncIssue Convert(SyncIssue syncIssue);

		/// <summary>
		/// Converts a collection of sync objects using the provided converters.
		/// </summary>
		/// <param name="syncObjects"> The sync objects to convert. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <param name="converters"> The converters to projects the sync objects. </param>
		/// <returns> The converted sync objects. </returns>
		public static IEnumerable<SyncObject> Convert(IEnumerable<SyncObject> syncObjects, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true, params SyncObjectConverter[] converters)
		{
			foreach (var syncObject in syncObjects)
			{
				yield return Convert(syncObject, type, excludePropertiesForSync, excludePropertiesForUpdate, converters);
			}
		}

		/// <summary>
		/// Converts a sync objects using the provided converters.
		/// </summary>
		/// <param name="syncObject"> The sync object to convert. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <param name="converters"> The converters to projects the sync objects. </param>
		/// <returns> The converted sync objects. </returns>
		public static SyncObject Convert(SyncObject syncObject, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true, params SyncObjectConverter[] converters)
		{
			// Cycle through each converter to process each object.
			foreach (var converter in converters)
			{
				// Ensure this converter can process the object.
				if (!converter.CanConvert(syncObject))
				{
					continue;
				}

				// Convert the object.
				return converter.Convert(syncObject, type, excludePropertiesForSync, excludePropertiesForUpdate);
			}

			return null;
		}
		
		/// <summary>
		/// Converts a sync issue using the provided converters.
		/// </summary>
		/// <param name="issue"> The sync issue to convert. </param>
		/// <param name="converters"> The converters to projects the sync issue. </param>
		/// <returns> The converted sync issue. </returns>
		public static SyncIssue Convert(SyncIssue issue, params SyncObjectConverter[] converters)
		{
			// Cycle through each converter to process each object.
			foreach (var converter in converters)
			{
				// Ensure this converter can process the object.
				if (!converter.CanConvert(issue))
				{
					continue;
				}

				// Convert the object.
				return converter.Convert(issue);
			}

			return null;
		}

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
		/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
		/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
		/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
		/// <param name="source"> The source sync entity with the updates. </param>
		/// <param name="destination"> The destination sync entity to be updated. </param>
		/// <param name="convert"> The convert method to do the conversion. </param>
		/// <param name="type"> The type of conversion. </param>
		/// <param name="excludePropertiesForSync"> If true excluded properties will not be set during sync. </param>
		/// <param name="excludePropertiesForUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public static bool Convert<T1, T2, T3, T4>(T1 source, T3 destination, Func<T1, T3, SyncConversionType, bool> convert, SyncConversionType type, bool excludePropertiesForSync = true, bool excludePropertiesForUpdate = true)
			where T1 : SyncEntity<T2>
			where T3 : SyncEntity<T4>
		{
			// Handle all one to one properties (same name & type) and all sync entity base properties.
			destination.UpdateWith(source, excludePropertiesForSync, excludePropertiesForUpdate);

			// Update will not set the sync ID
			destination.SyncId = source.SyncId;

			// Optional convert to do additional conversions
			return convert?.Invoke(source, destination, type) ?? true;
		}

		#endregion
	}
}