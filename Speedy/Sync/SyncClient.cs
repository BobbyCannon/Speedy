#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Speedy.Configuration;
using Speedy.Exceptions;
using Speedy.Logging;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public class SyncClient : ISyncClient
	{
		#region Fields

		private readonly ISyncableDatabaseProvider _provider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync client.
		/// </summary>
		public SyncClient(string name, ISyncableDatabaseProvider provider)
			: this(name, Guid.NewGuid(), provider)
		{
		}

		/// <summary>
		/// Instantiates a sync client.
		/// </summary>
		public SyncClient(string name, Guid sessionId, ISyncableDatabaseProvider provider)
		{
			_provider = provider;
			Name = name;
			SessionId = sessionId;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the sync client.
		/// </summary>
		public string Name { get; }

		/// <inheritdoc />
		public Guid SessionId { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public IEnumerable<SyncIssue> ApplyChanges(IEnumerable<SyncObject> changes)
		{
			return ApplyChanges(changes, false);
		}

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		public IEnumerable<SyncIssue> ApplyCorrections(IEnumerable<SyncObject> corrections)
		{
			return ApplyChanges(corrections, true);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			using (var database = _provider.GetDatabase())
			{
				return database.GetSyncTombstones(x => x.CreatedOn >= request.Since && x.CreatedOn < request.Until).Count()
					+ database.GetSyncableRepositories().Sum(repository => repository.GetChangeCount(request.Since, request.Until));
			}
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			var response = new List<SyncObject>();
			var currentSkippedCount = 0;

			using (var database = _provider.GetDatabase())
			{
				foreach (var repository in database.GetSyncableRepositories())
				{
					var changeCount = repository.GetChangeCount(request.Since, request.Until);
					if (changeCount + currentSkippedCount <= request.Skip)
					{
						currentSkippedCount += changeCount;
						continue;
					}

					var items = repository.GetChanges(request.Since, request.Until, request.Skip - currentSkippedCount, request.Take).ToList();
					response.AddRange(items);
					currentSkippedCount += items.Count;

					if (response.Count >= request.Take)
					{
						return response;
					}
				}

				var tombstoneQuery = database.GetSyncTombstones(x => x.CreatedOn >= request.Since && x.CreatedOn < request.Until);
				var tombstoneCount = tombstoneQuery.Count();
				if (tombstoneCount + currentSkippedCount <= request.Skip)
				{
					return response;
				}

				tombstoneQuery = tombstoneQuery
					.OrderBy(x => x.CreatedOn)
					.ThenBy(x => x.Id)
					.AsQueryable();

				if (request.Skip - currentSkippedCount > 0)
				{
					tombstoneQuery = tombstoneQuery.Skip(request.Skip - currentSkippedCount);
				}

				var tombStones = tombstoneQuery
					.Take(request.Take)
					.ToList()
					.Select(x => x.ToSyncObject())
					.ToList();

				response.AddRange(tombStones);
				return response;
			}
		}

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		public IEnumerable<SyncObject> GetCorrections(IEnumerable<SyncIssue> issues)
		{
			var response = new List<SyncObject>();

			using (var database = _provider.GetDatabase())
			{
				foreach (var issue in issues)
				{
					switch (issue.IssueType)
					{
						default:
							// Assuming this is because this entity or a relationship it depends on was deleted but then used 
							// in another client or server. This means we should sync it again.
							var repository = database.GetSyncableRepository(Type.GetType(issue.TypeName));
							var entity = repository.Read(issue.Id);

							if (entity != null)
							{
								response.Add(entity.ToSyncObject());
							}
							break;
					}
				}

				return response;
			}
		}

		/// <inheritdoc />
		public ISyncableDatabase GetDatabase()
		{
			return _provider.GetDatabase();
		}

		/// <inheritdoc />
		public T GetDatabase<T>() where T : class, ISyncableDatabase, IDatabase
		{
			return (T) _provider.GetDatabase();
		}

		private IEnumerable<SyncIssue> ApplyChanges(IEnumerable<SyncObject> changes, bool corrections)
		{
			var groups = changes.GroupBy(x => x.TypeName).OrderBy(x => x.Key);

			if (_provider.Options.SyncOrder.Any())
			{
				var order = _provider.Options.SyncOrder;
				groups = groups.OrderBy(x => x.Key == order[0]);
				groups = order.Skip(1).Aggregate(groups, (current, typeName) => current.ThenBy(x => x.Key == typeName));
			}

			var response = new List<SyncIssue>();

			groups.ForEach(x => ProcessSyncObjects(_provider, x.Where(y => y.Status != SyncObjectStatus.Deleted), response, corrections));
			groups.Reverse().ForEach(x => ProcessSyncObjects(_provider, x.Where(y => y.Status == SyncObjectStatus.Deleted), response, corrections));

			return response;
		}

		private static IEnumerable<Relationship> GetRelationshipConfigurations(SyncEntity entity)
		{
			var syncEntityType = typeof(SyncEntity);
			var properties = entity.GetRealType().GetProperties();
			var syncProperties = properties
				.Where(x => syncEntityType.IsAssignableFrom(x.PropertyType))
				.Select(x => new
				{
					IdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "Id"),
					SyncIdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "SyncId"),
					Type = x.PropertyType
				})
				.ToList();

			var response = syncProperties
				.Where(x => x.IdProperty != null)
				.Where(x => x.SyncIdProperty != null)
				.Select(x => new Relationship
				{
					IdPropertyInfo = x.IdProperty,
					SyncId = (Guid?) x.SyncIdProperty.GetValue(entity),
					Type = x.Type
				})
				.ToList();

			return response;
		}

		private void ProcessSyncObject(SyncObject syncObject, ISyncableDatabase database, bool correction)
		{
			Logger.Instance.Write(SessionId, correction ? $"Processing sync object correction {syncObject.SyncId}." : $"Processing sync object {syncObject.SyncId}.", EventLevel.Verbose);

			var syncEntity = syncObject.ToSyncEntity();
			var tombstone = database.GetSyncTombstones(x => x.SyncId == syncEntity.SyncId).FirstOrDefault();
			if (tombstone != null)
			{
				if (!correction)
				{
					return;
				}

				database.RemoveSyncTombstones(x => x.SyncId == syncEntity.SyncId);
			}

			var type = syncEntity.GetType();
			var repository = database.GetSyncableRepository(type);
			if (repository == null)
			{
				throw new InvalidDataException("Failed to find a syncable repository for the entity.");
			}

			var foundEntity = repository.Read(syncEntity.SyncId);
			var syncStatus = syncObject.Status;

			if (foundEntity != null && syncObject.Status == SyncObjectStatus.Added)
			{
				syncStatus = SyncObjectStatus.Modified;
			}
			else if (foundEntity == null && syncObject.Status == SyncObjectStatus.Modified)
			{
				syncStatus = SyncObjectStatus.Added;
			}

			switch (syncStatus)
			{
				case SyncObjectStatus.Added:
					syncEntity.Id = 0;
					UpdateLocalRelationships(syncEntity, database);
					repository.Add(syncEntity);
					break;

				case SyncObjectStatus.Modified:
					UpdateLocalRelationships(foundEntity, database);

					if (foundEntity?.ModifiedOn < syncEntity.ModifiedOn || correction)
					{
						foundEntity?.Update(syncEntity);
					}
					break;

				case SyncObjectStatus.Deleted:
					if (foundEntity != null)
					{
						repository.Remove(foundEntity);
					}
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ProcessSyncObjects(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var syncObjectList = syncObjects.ToList();

			try
			{
				using (var database = provider.GetDatabase())
				{
					database.Options.MaintainDates = false;
					syncObjectList.ForEach(x => ProcessSyncObject(x, database, corrections));
					database.SaveChanges();
				}
			}
			catch
			{
				Logger.Instance.Write(SessionId, "Failed to process sync objects in the batch.");
				ProcessSyncObjectsIndividually(provider, syncObjectList, issues, corrections);
			}
		}

		private void ProcessSyncObjectsIndividually(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			foreach (var syncObject in syncObjects)
			{
				try
				{
					using (var database = provider.GetDatabase())
					{
						database.Options.MaintainDates = false;
						ProcessSyncObject(syncObject, database, corrections);
						database.SaveChanges();
					}
				}
				catch (SyncIssueException ex)
				{
					ex.Issues.ForEach(issues.Add);
					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
				}
				catch (InvalidOperationException)
				{
					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
				}
				catch (Exception ex)
				{
					var details = ex.ToDetailedString();

					// Cannot catch the DbUpdateException without reference EntityFramework.
					if (details.Contains("conflicted with the FOREIGN KEY constraint")
						|| details.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
					{
						issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
						continue;
					}

					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.Unknown, TypeName = syncObject.TypeName });
				}
			}
		}

		/// <summary>
		/// Updates the entities local relationships.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="database"> The database with the relationship repositories. </param>
		/// <exception cref="SyncIssueException"> An exception will all sync issues. </exception>
		private static void UpdateLocalRelationships(SyncEntity entity, ISyncableDatabase database)
		{
			var response = new List<SyncIssue>();

			foreach (var relationship in GetRelationshipConfigurations(entity))
			{
				if (!relationship.SyncId.HasValue)
				{
					continue;
				}

				var foundEntity = database.GetSyncableRepository(relationship.Type)?.Read(relationship.SyncId.Value);
				if (foundEntity != null)
				{
					relationship.IdPropertyInfo.SetValue(entity, foundEntity.Id);
					continue;
				}

				response.Add(new SyncIssue { Id = relationship.SyncId.Value, IssueType = SyncIssueType.RelationshipConstraint, TypeName = relationship.Type.ToAssemblyName() });
			}

			if (response.Any(x => x != null))
			{
				throw new SyncIssueException("This entity has relationship issues.", response.Where(x => x != null));
			}
		}

		#endregion
	}
}