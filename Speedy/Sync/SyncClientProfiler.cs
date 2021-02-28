#region References

using System;
using System.Text;
using Speedy.Profiling;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Profiler for a the sync client
	/// </summary>
	public class SyncClientProfiler
	{
		#region Fields

		private readonly string _name;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync client profiler.
		/// </summary>
		/// <param name="name"> The name of the sync client the profiler is for. </param>
		public SyncClientProfiler(string name)
		{
			_name = name;

			ApplyChanges = new Timer();
			GetChanges = new Timer();
			GetChangeCount = new Timer();
			ProcessSyncObjectsSyncObjectsToList = new Timer();
			ProcessSyncObject = new Timer();
			ProcessSyncObjectAdded = new Timer();
			ProcessSyncObjectDeleted = new Timer();
			ProcessSyncObjectModified = new Timer();
			ProcessSyncObjectReadEntity = new Timer();
			ProcessSyncObjects = new Timer();
			ProcessSyncObjectsGetDatabase = new Timer();
			ProcessSyncObjectsSaveDatabase = new Timer();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The overall processing time for SyncClient.ApplyChanges.
		/// </summary>
		public Timer ApplyChanges { get; }

		/// <summary>
		/// The overall processing time for <seealso cref="SyncClient.GetChangeCount" />.
		/// </summary>
		public Timer GetChangeCount { get; }

		/// <summary>
		/// The overall processing time for <seealso cref="SyncClient.GetChanges" />.
		/// </summary>
		public Timer GetChanges { get; }

		/// <summary>
		/// The overall processing time for <seealso cref="SyncClient.ProcessSyncObject" />.
		/// </summary>
		public Timer ProcessSyncObject { get; }

		/// <summary>
		/// The "Added" portion of processing time for <seealso cref="SyncClient.ProcessSyncObject" />.
		/// </summary>
		public Timer ProcessSyncObjectAdded { get; }

		/// <summary>
		/// The "Deleted" portion of processing time for <seealso cref="SyncClient.ProcessSyncObject" />.
		/// </summary>
		public Timer ProcessSyncObjectDeleted { get; }

		/// <summary>
		/// The "Modified" portion of processing time for <seealso cref="SyncClient.ProcessSyncObject" />.
		/// </summary>
		public Timer ProcessSyncObjectModified { get; }

		/// <summary>
		/// The "ReadEntity" portion of processing time for <seealso cref="SyncClient.ProcessSyncObject" />.
		/// </summary>
		public Timer ProcessSyncObjectReadEntity { get; }

		/// <summary>
		/// The overall processing time for <seealso cref="SyncClient.ProcessSyncObjects" />.
		/// </summary>
		public Timer ProcessSyncObjects { get; }

		/// <summary>
		/// The "GetDatabase" portion of processing time for <seealso cref="SyncClient.ProcessSyncObjects" />.
		/// </summary>
		public Timer ProcessSyncObjectsGetDatabase { get; }

		/// <summary>
		/// The "SaveDatabase" portion of processing time for <seealso cref="SyncClient.ProcessSyncObjects" />.
		/// </summary>
		public Timer ProcessSyncObjectsSaveDatabase { get; }

		/// <summary>
		/// The "SyncObjectsToList" portion of processing time for <seealso cref="SyncClient.ProcessSyncObjects" />.
		/// </summary>
		public Timer ProcessSyncObjectsSyncObjectsToList { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Converts the profile results into a human readable string.
		/// </summary>
		/// <param name="totalTime"> The overall time to process. This is for generating percent value. </param>
		/// <returns> The human readable string for the profiler results. </returns>
		public string ToString(TimeSpan totalTime)
		{
			var builder = new StringBuilder();
			builder.AppendLine(_name);
			builder.AppendLine($"\tGetChanges {GetChanges} : {Percent(totalTime, GetChanges)}");
			builder.AppendLine($"\tGetChangesCount {GetChangeCount} : {Percent(totalTime, GetChangeCount)}");
			builder.AppendLine($"\tApplyChanges {ApplyChanges} : {Percent(totalTime, ApplyChanges)}");
			builder.AppendLine($"\t\tProcessSyncObjects {ProcessSyncObjects} : {Percent(totalTime, ProcessSyncObjects)}");
			builder.AppendLine($"\t\t\tProcessSyncObject {ProcessSyncObject} : {Percent(totalTime, ProcessSyncObject)}");
			builder.AppendLine($"\t\t\t\tProcessSyncObject::Added {ProcessSyncObjectAdded} : {Percent(totalTime, ProcessSyncObjectAdded)}");
			builder.AppendLine($"\t\t\t\tProcessSyncObject::Modified {ProcessSyncObjectModified} : {Percent(totalTime, ProcessSyncObjectModified)}");
			builder.AppendLine($"\t\t\t\tProcessSyncObject::ReadEntity {ProcessSyncObjectReadEntity} : {Percent(totalTime, ProcessSyncObjectReadEntity)}");
			builder.AppendLine($"\t\tProcessSyncObjects::GetDatabase {ProcessSyncObjectsGetDatabase} : {Percent(totalTime, ProcessSyncObjectsGetDatabase)}");
			builder.AppendLine($"\t\tProcessSyncObjects::SaveDatabase {ProcessSyncObjectsSaveDatabase} : {Percent(totalTime, ProcessSyncObjectsSaveDatabase)}");
			builder.AppendLine($"\t\tProcessSyncObjects::ToList {ProcessSyncObjectsSyncObjectsToList} : {Percent(totalTime, ProcessSyncObjectsSyncObjectsToList)}");
			return builder.ToString();
		}

		private string Percent(TimeSpan total, Timer partial)
		{
			return $"{(double) partial.Elapsed.Ticks / total.Ticks * 100:0.00}%";
		}

		#endregion
	}
}