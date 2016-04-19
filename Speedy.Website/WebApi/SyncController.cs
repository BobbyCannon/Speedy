#region References

using System;
using System.Collections.Generic;
using System.Web.Http;
using Speedy.Samples;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	public class SyncController : BaseController, ISyncServer
	{
		#region Constructors

		public SyncController() : this(new EntityFrameworkContosoDatabase())
		{
		}

		public SyncController(IContosoDatabase database) : base(database)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		[HttpPost]
		public DateTime ApplyChanges([FromBody] IEnumerable<SyncObject> changes)
		{
			var response = DateTime.UtcNow;
			Database.ApplySyncChanges(changes);
			Database.SaveChanges();
			return response;
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of changes from the server. </returns>
		[HttpPost]
		public IEnumerable<SyncObject> GetChanges([FromBody] DateTime since)
		{
			return Database.GetSyncChanges(since);
		}

		#endregion
	}
}