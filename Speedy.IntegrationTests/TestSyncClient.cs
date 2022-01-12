#region References

using System;
using System.Collections.Generic;
using Speedy.Net;
using Speedy.Sync;

#endregion

namespace Speedy.IntegrationTests
{
	public class TestSyncClient : ISyncClient
	{
		#region Constructors

		public TestSyncClient(string name)
		{
			AppliedChanges = new List<ServiceRequest<SyncObject>>();
			AppliedCorrections = new List<ServiceRequest<SyncObject>>();
			Changes = new List<ServiceResult<SyncObject>>();
			ChangesOffset = 0;
			Corrections = new List<ServiceResult<SyncObject>>();
			CorrectionsOffset = 0;
			DatabaseProvider = new SyncableDatabaseProvider((x, y) => null, new DatabaseOptions(), new DatabaseKeyCache());
			Name = name;
			Options = new SyncClientOptions();
			Profiler = new SyncClientProfiler(name);
			Statistics = new SyncStatistics();
			SyncSession = new SyncSession();
			SyncOptions = new SyncOptions();
		}

		#endregion

		#region Properties

		public List<ServiceRequest<SyncObject>> AppliedChanges { get; }

		public List<ServiceRequest<SyncObject>> AppliedCorrections { get; }

		public List<ServiceResult<SyncObject>> Changes { get; }

		public int ChangesOffset { get; private set; }

		public List<ServiceResult<SyncObject>> Corrections { get; }

		public int CorrectionsOffset { get; private set; }

		public ISyncableDatabaseProvider DatabaseProvider { get; }

		public SyncClientIncomingConverter IncomingConverter { get; set; }

		public string Name { get; }

		public SyncClientOptions Options { get; }

		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		public SyncClientProfiler Profiler { get; }

		public SyncStatistics Statistics { get; }

		public SyncOptions SyncOptions { get; }

		public SyncSession SyncSession { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			
		}

		public ServiceResult<SyncIssue> ApplyChanges(Guid sessionId, ServiceRequest<SyncObject> changes)
		{
			AppliedChanges.Add(changes);
			return new ServiceResult<SyncIssue>();
		}

		public ServiceResult<SyncIssue> ApplyCorrections(Guid sessionId, ServiceRequest<SyncObject> corrections)
		{
			AppliedCorrections.Add(corrections);
			return new ServiceResult<SyncIssue>();
		}

		public SyncSession BeginSync(Guid sessionId, SyncOptions options)
		{
			return new() { Id = sessionId, StartedOn = TimeService.UtcNow };
		}

		public SyncStatistics EndSync(Guid sessionId)
		{
			return Statistics;
		}

		public ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request)
		{
			return Changes.Count > ChangesOffset ? Changes[ChangesOffset++] : new ServiceResult<SyncObject>();
		}

		public ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues)
		{
			return Corrections.Count > CorrectionsOffset ? Corrections[CorrectionsOffset++] : new ServiceResult<SyncObject>();
		}

		public ISyncableDatabase GetDatabase()
		{
			return null;
		}

		T ISyncClient.GetDatabase<T>()
		{
			return null;
		}

		#endregion
	}
}