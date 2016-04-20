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
		public void ApplyChanges([FromBody] IEnumerable<SyncObject> changes)
		{
			Database.ApplySyncChanges(changes);
			Database.SaveChanges();
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

		#endregion
	}
}