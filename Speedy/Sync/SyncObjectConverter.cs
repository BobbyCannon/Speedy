#region References

using System;

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
	public class SyncObjectIncomingConverter<T1, T2, T3, T4> : SyncObjectIncomingConverter
		where T1 : SyncEntity<T2>
		where T3 : SyncEntity<T4>
	{
		#region Fields

		private readonly Action<T1, T3> _convert;
		private readonly Func<T3, T3, SyncObjectStatus, bool> _update;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of a converter.
		/// </summary>
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		/// <param name="update"> An optional update method to do some additional updating. </param>
		public SyncObjectIncomingConverter(Action<T1, T3> convert = null, Func<T3, T3, SyncObjectStatus, bool> update = null)
			: base(typeof(T1).GetRealType().ToAssemblyName(), typeof(T3).GetRealType().ToAssemblyName())
		{
			_convert = convert;
			_update = update;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanUpdate(ISyncEntity syncEntity)
		{
			return syncEntity is T3;
		}

		/// <inheritdoc />
		public override SyncObject Convert(SyncObject syncObject, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync)
		{
			return Convert<T1, T2, T3, T4>(syncObject, _convert, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync);
		}

		/// <inheritdoc />
		public override bool Update(ISyncEntity source, ISyncEntity destination, SyncObjectStatus status, bool excludePropertiesForSyncUpdate)
		{
			return Update<T3, T4>((T3) source, (T3) destination, _update, status, excludePropertiesForSyncUpdate);
		}

		#endregion
	}

	/// <summary>
	/// Represents an outgoing object converter.
	/// </summary>
	/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
	/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
	/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
	/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
	public class SyncObjectOutgoingConverter<T1, T2, T3, T4> : SyncObjectOutgoingConverter
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
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		public SyncObjectOutgoingConverter(Action<T1, T3> convert = null)
			: base(typeof(T1).GetRealType().ToAssemblyName(), typeof(T3).GetRealType().ToAssemblyName())
		{
			_convert = convert;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool CanUpdate(ISyncEntity syncEntity)
		{
			return false;
		}

		/// <inheritdoc />
		public override SyncObject Convert(SyncObject syncObject, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync)
		{
			return Convert<T1, T2, T3, T4>(syncObject, _convert, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync);
		}

		/// <inheritdoc />
		public override bool Update(ISyncEntity source, ISyncEntity destination, SyncObjectStatus status, bool excludePropertiesForSyncUpdate)
		{
			return false;
		}

		#endregion
	}

	/// <summary>
	/// Represents an incoming object converter.
	/// </summary>
	public abstract class SyncObjectIncomingConverter : SyncObjectConverter
	{
		#region Constructors

		/// <summary>
		/// Instantiate an incoming object converter.
		/// </summary>
		protected SyncObjectIncomingConverter(string sourceName, string destinationName) : base(sourceName, destinationName)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents an outgoing object converter.
	/// </summary>
	public abstract class SyncObjectOutgoingConverter : SyncObjectConverter
	{
		#region Constructors

		/// <summary>
		/// Instantiate an outgoing object converter.
		/// </summary>
		protected SyncObjectOutgoingConverter(string sourceName, string destinationName) : base(sourceName, destinationName)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents an object converter.
	/// </summary>
	public abstract class SyncObjectConverter
	{
		#region Constructors

		/// <summary>
		/// Instantiate an object converter.
		/// </summary>
		protected SyncObjectConverter(string sourceName, string destinationName)
		{
			SourceName = sourceName;
			DestinationName = destinationName;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The destination type name.
		/// </summary>
		protected string DestinationName { get; }

		/// <summary>
		/// The source type name.
		/// </summary>
		protected string SourceName { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Test a sync object name to see if this converter can convert this object.
		/// </summary>
		/// <param name="name"> The sync object name to test. </param>
		/// <returns> True if the sync object can be converted or false if otherwise. </returns>
		public bool CanConvert(string name)
		{
			return SourceName == name;
		}
		
		/// <summary>
		/// Test a sync object to see if this converter can convert this object.
		/// </summary>
		/// <param name="syncObject"> The sync object to test. </param>
		/// <returns> True if the sync object can be converted or false if otherwise. </returns>
		public bool CanConvert(SyncObject syncObject)
		{
			return CanConvert(syncObject.TypeName);
		}

		/// <summary>
		/// Test a sync issue to see if this converter can convert this object.
		/// </summary>
		/// <param name="syncIssue"> The sync issue to test. </param>
		/// <returns> True if the sync issue can be converted or false if otherwise. </returns>
		public bool CanConvert(SyncIssue syncIssue)
		{
			return CanConvert(syncIssue.TypeName);
		}

		/// <summary>
		/// Test a sync entity to see if this converter can update this object.
		/// </summary>
		/// <param name="syncEntity"> The sync entity to test. </param>
		/// <returns> True if the sync entity can be updated or false if otherwise. </returns>
		public abstract bool CanUpdate(ISyncEntity syncEntity);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <param name="syncObject"> The sync object to process. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		public abstract SyncObject Convert(SyncObject syncObject, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync);

		/// <summary>
		/// Convert this sync issue to a different sync object
		/// </summary>
		/// <param name="syncIssue"> The sync issue to process. </param>
		/// <returns> The converted sync issue in a sync issue format. </returns>
		public SyncIssue Convert(SyncIssue syncIssue)
		{
			return syncIssue.Convert(DestinationName);
		}

		/// <summary>
		/// Updates this sync object with another object.
		/// </summary>
		/// <param name="source"> The entity with the updates. </param>
		/// <param name="destination"> The destination sync entity to be updated. </param>
		/// <param name="status"> The status of the update. </param>
		/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> Return true if the entity was updated and should be saved. </returns>
		public abstract bool Update(ISyncEntity source, ISyncEntity destination, SyncObjectStatus status, bool excludePropertiesForSyncUpdate);

		/// <summary>
		/// Convert this sync object to a different sync object
		/// </summary>
		/// <typeparam name="T1"> The sync entity type to convert from. </typeparam>
		/// <typeparam name="T2"> The primary key of the entity to convert from. </typeparam>
		/// <typeparam name="T3"> The sync entity type to convert to. </typeparam>
		/// <typeparam name="T4"> The primary key of the entity to convert to. </typeparam>
		/// <param name="syncObject"> The sync object to be converted. </param>
		/// <param name="convert"> An optional convert method to do some additional conversion. </param>
		/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
		/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
		/// <returns> The converted sync entity in a sync object format. </returns>
		protected static SyncObject Convert<T1, T2, T3, T4>(SyncObject syncObject, Action<T1, T3> convert, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync)
			where T1 : SyncEntity<T2>
			where T3 : SyncEntity<T4>
		{
			var source = syncObject.ToSyncEntity<T1, T2>();
			var destination = Activator.CreateInstance<T3>();

			// Handle all one to one properties (same name & type) and all sync entity base properties.
			destination.UpdateWith(source, excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, false);

			// Update will not set the sync ID
			destination.SyncId = source.SyncId;

			// Optional convert to do additional conversions
			convert?.Invoke(source, destination);

			// Keep status because it should be the same, ex Deleted
			var response = destination.ToSyncObject();
			response.Status = syncObject.Status;
			return response;
		}

		/// <summary>
		/// Updates this sync object with another object.
		/// </summary>
		/// <typeparam name="T1"> The sync entity type to process. </typeparam>
		/// <typeparam name="T2"> The primary key of the sync entity. </typeparam>
		/// <param name="source"> The entity with the updates. </param>
		/// <param name="destination"> The destination sync entity to be updated. </param>
		/// <param name="action"> The function to do the updating. </param>
		/// <param name="status"> The status of the update. </param>
		/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
		/// <returns> Return true if the entity was updated and should be saved. </returns>
		protected static bool Update<T1, T2>(T1 source, T1 destination, Func<T1, T1, SyncObjectStatus, bool> action, SyncObjectStatus status, bool excludePropertiesForSyncUpdate)
			where T1 : SyncEntity<T2>
		{
			if (destination == null)
			{
				destination = Activator.CreateInstance<T1>();
			}

			// Handle all one to one properties (same name & type) and all sync entity base properties.
			destination.UpdateWith(source, false, false, excludePropertiesForSyncUpdate);

			// Update will not set the sync ID
			destination.SyncId = source.SyncId;

			// Optional convert to do additional conversions
			return action?.Invoke(source, destination, status) ?? true;
		}

		#endregion
	}
}