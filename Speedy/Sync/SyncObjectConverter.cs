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

		private readonly Action<T1, T3> _convert;

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
		public SyncObjectConverter(Action<T1, T3> convert)
		{
			_convert = convert;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanConvert(SyncObject syncObject)
		{
			var type = Type.GetType(syncObject.TypeName);
			if (type == null)
			{
				return false;
			}

			if (!typeof(T1).IsAssignableFrom(type))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override SyncObject Convert(SyncObject syncObject, bool allowSyncExclusions = true, bool allowUpdateExclusions = true)
		{
			return syncObject.Convert<T1, T2, T3, T4>(_convert, allowSyncExclusions, allowUpdateExclusions);
		}

		#endregion
	}

	/// <summary>
	/// Represents an object converter.
	/// </summary>
	public abstract class SyncObjectConverter
	{
		#region Methods

		/// <summary>
		/// Test a sync object to see if this converter can process this object.
		/// </summary>
		/// <param name="syncObject"> The sync object to test. </param>
		/// <returns> True if the sync object can be process or false if otherwise. </returns>
		public abstract bool CanConvert(SyncObject syncObject);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <param name="syncObject"> The sync object to process. </param>
		/// <param name="allowSyncExclusions"> If true excluded sync properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="allowUpdateExclusions"> If true excluded update properties will not be updated otherwise all matching properties will be updated. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public abstract SyncObject Convert(SyncObject syncObject, bool allowSyncExclusions = true, bool allowUpdateExclusions = true);

		/// <summary>
		/// Converts a collection of sync objects using the provided converters.
		/// </summary>
		/// <param name="syncObjects"> The sync objects to convert. </param>
		/// <param name="allowSyncExclusions"> If true excluded sync properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="allowUpdateExclusions"> If true excluded update properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="converters"> The converters to projects the sync objects. </param>
		/// <returns> The converted sync objects. </returns>
		public static IEnumerable<SyncObject> Convert(IEnumerable<SyncObject> syncObjects, bool allowSyncExclusions = true, bool allowUpdateExclusions = true, params SyncObjectConverter[] converters)
		{
			foreach (var syncObject in syncObjects)
			{
				yield return Convert(syncObject, allowSyncExclusions, allowUpdateExclusions, converters);
			}
		}

		/// <summary>
		/// Converts a sync objects using the provided converters.
		/// </summary>
		/// <param name="syncObject"> The sync object to convert. </param>
		/// <param name="allowSyncExclusions"> If true excluded sync properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="allowUpdateExclusions"> If true excluded update properties will not be updated otherwise all matching properties will be updated. </param>
		/// <param name="converters"> The converters to projects the sync objects. </param>
		/// <returns> The converted sync objects. </returns>
		public static SyncObject Convert(SyncObject syncObject, bool allowSyncExclusions = true, bool allowUpdateExclusions = true, params SyncObjectConverter[] converters)
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
				return converter.Convert(syncObject, allowSyncExclusions, allowUpdateExclusions);
			}

			return null;
		}

		#endregion
	}
}