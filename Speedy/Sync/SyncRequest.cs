#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// The details to ask a sync client for changes.
	/// </summary>
	public class SyncRequest : ServiceRequest<SyncObject>
	{
		#region Constructors

		/// <summary>
		/// Instantiates a sync request.
		/// </summary>
		public SyncRequest() : this(new SyncObject[0])
		{
		}

		/// <summary>
		/// Instantiates a sync request.
		/// </summary>
		public SyncRequest(params SyncObject[] collection) : this(collection.ToList())
		{
		}

		/// <summary>
		/// Instantiates a sync request.
		/// </summary>
		public SyncRequest(IEnumerable<SyncObject> collection) : base(collection)
		{
			Reset();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The start date and time to get changes for.
		/// </summary>
		public DateTime Since { get; set; }

		/// <summary>
		/// The end date and time to get changes for.
		/// </summary>
		public DateTime Until { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Resets the filter back to defaults.
		/// </summary>
		public void Reset()
		{
			Since = DateTime.MinValue;
			Until = TimeService.UtcNow;
		}

		#endregion
	}
}