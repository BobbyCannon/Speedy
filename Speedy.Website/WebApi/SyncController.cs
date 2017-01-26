#region References

using System;
using System.Collections.Generic;
using System.Web.Http;
using Speedy.Samples;
using Speedy.Samples.EntityFramework;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	public class SyncController : BaseController
	{
		#region Fields

		private readonly SyncClient _client;

		#endregion

		#region Constructors

		public SyncController()
			: this(new EntityFrameworkContosoDatabaseProvider())
		{
		}

		public SyncController(IContosoDatabaseProvider provider)
			: base(provider.GetDatabase())
		{
			_client = new SyncClient(Guid.NewGuid().ToString(), provider);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="id"> The ID of the session. </param>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		[HttpPost]
		public IEnumerable<SyncIssue> ApplyChanges([FromUri] Guid id, [FromBody] IEnumerable<SyncObject> changes)
		{
			_client.SessionId = id;
			return _client.ApplyChanges(changes);
		}

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="id"> The ID of the session. </param>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		[HttpPost]
		public IEnumerable<SyncIssue> ApplyCorrections([FromUri] Guid id, [FromBody] IEnumerable<SyncObject> corrections)
		{
			_client.SessionId = id;
			return _client.ApplyCorrections(corrections);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="id"> The ID of the session. </param>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		[HttpPost]
		public int GetChangeCount([FromUri] Guid id, [FromBody] SyncRequest request)
		{
			_client.SessionId = id;
			return _client.GetChangeCount(request);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="id"> The ID of the session. </param>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		[HttpPost]
		public IEnumerable<SyncObject> GetChanges([FromUri] Guid id, [FromBody] SyncRequest request)
		{
			_client.SessionId = id;
			return _client.GetChanges(request);
		}

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="id"> The ID of the session. </param>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		[HttpPost]
		public IEnumerable<SyncObject> GetCorrections([FromUri] Guid id, [FromBody] IEnumerable<SyncIssue> issues)
		{
			_client.SessionId = id;
			return _client.GetCorrections(issues);
		}

		#endregion
	}
}