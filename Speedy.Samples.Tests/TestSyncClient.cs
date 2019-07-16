#region References

using System;
using System.Collections.Generic;
using Speedy.Net;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Tests
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
		}

		#endregion

		#region Properties

		public List<ServiceRequest<SyncObject>> AppliedChanges { get; set; }

		public List<ServiceRequest<SyncObject>> AppliedCorrections { get; set; }

		public List<ServiceResult<SyncObject>> Changes { get; set; }

		public int ChangesOffset { get; private set; }

		public List<ServiceResult<SyncObject>> Corrections { get; set; }

		public int CorrectionsOffset { get; private set; }

		public SyncClientConverter IncomingConverter { get; set; }

		public string Name { get; }

		public SyncClientOptions Options { get; set; }

		public SyncClientConverter OutgoingConverter { get; set; }

		public SyncStatistics Statistics { get; set; }

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

		public void BeginSync(Guid sessionId, SyncOptions options)
		{
		}

		public void EndSync(Guid sessionId)
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
			throw new NotImplementedException();
		}

		T ISyncClient.GetDatabase<T>()
		{
			return null;
		}

		#endregion
	}
}