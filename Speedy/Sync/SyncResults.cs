using System;

namespace Speedy.Sync
{
	/// <summary>
	/// The results of the sync.
	/// </summary>
	public class SyncResults<T> : Bindable
	{
		#region Properties

		/// <summary>
		/// The sync client for the client.
		/// </summary>
		public ISyncClient Client { get; set; }

		/// <summary>
		/// The elapsed time for the sync.
		/// </summary>
		public TimeSpan Elapsed { get; set; }

		/// <summary>
		/// The sync options.
		/// </summary>
		public SyncOptions Options { get; set; }

		/// <summary>
		/// The sync client for the server.
		/// </summary>
		public ISyncClient Server { get; set; }

		/// <summary>
		/// The Type for the sync.
		/// </summary>
		public T SyncType { get; set; }

		#endregion
	}
}