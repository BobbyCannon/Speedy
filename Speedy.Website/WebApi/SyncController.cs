#region References

using System;
using System.Collections.Generic;
using System.Web.Http;
using Speedy.Samples;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	public class SyncController : BaseController, ISyncClient
	{
		#region Constructors

		public SyncController()
			: this(new EntityFrameworkContosoDatabase())
		{
		}

		public SyncController(IContosoDatabase database)
			: base(database)
		{
			Name = Guid.NewGuid().ToString();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the sync client.
		/// </summary>
		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		[HttpPost]
		public IEnumerable<SyncIssue> ApplyChanges([FromBody] IEnumerable<SyncObject> changes)
		{
			return Database.ApplySyncChanges(changes);
		}

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		[HttpPost]
		public IEnumerable<SyncIssue> ApplyCorrections([FromBody] IEnumerable<SyncObject> corrections)
		{
			return Database.ApplySyncCorrections(corrections);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		[HttpPost]
		public int GetChangeCount([FromBody] SyncRequest request)
		{
			return Database.GetSyncChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		[HttpPost]
		public IEnumerable<SyncObject> GetChanges([FromBody] SyncRequest request)
		{
			return Database.GetSyncChanges(request);
		}

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		[HttpPost]
		public IEnumerable<SyncObject> GetCorrections([FromBody] IEnumerable<SyncIssue> issues)
		{
			return Database.GetSyncCorrections(issues);
		}

		#endregion
	}
}