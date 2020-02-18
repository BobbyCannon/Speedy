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
			Name = name;
			Options = new SyncClientOptions();
			Statistics = new SyncStatistics();
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

		public SyncClientIncomingConverter IncomingConverter { get; set; }

		public string Name { get; }

		public SyncClientOptions Options { get; }

		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		public SyncStatistics Statistics { get; }

		public SyncOptions SyncOptions { get; }

		#endregion

		#region Methods

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
			return new SyncSession { Id = sessionId, StartedOn = TimeService.UtcNow };
		}

		public void EndSync(SyncSession session)
		{
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

		public void UpdateOptions(Guid sessionId, SyncClientOptions options)
		{
		}

		T ISyncClient.GetDatabase<T>()
		{
			return null;
		}

		#endregion
	}
}