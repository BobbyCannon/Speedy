#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Speedy.Configuration;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public class SyncClient : ISyncClient
	{
		#region Fields

		private int _changeCount;
		private readonly ISyncableDatabaseProvider _provider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync client.
		/// </summary>
		public SyncClient(string name, ISyncableDatabaseProvider provider)
		{
			Name = name;
			Options = new SyncClientOptions();
			Statistics = new SyncStatistics();

			_changeCount = -1;
			_provider = provider;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public SyncClientIncomingConverter IncomingConverter { get; set; }

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public SyncClientOptions Options { get; protected set; }

		/// <inheritdoc />
		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		/// <inheritdoc />
		public SyncStatistics Statistics { get; }

		/// <inheritdoc />
		public SyncOptions SyncOptions { get; protected set; }

		/// <inheritdoc />
		public SyncSession SyncSession { get; protected set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public virtual ServiceResult<SyncIssue> ApplyChanges(Guid sessionId, ServiceRequest<SyncObject> changes)
		{
			return ApplyChanges(changes, false);
		}

		/// <inheritdoc />
		public virtual ServiceResult<SyncIssue> ApplyCorrections(Guid sessionId, ServiceRequest<SyncObject> corrections)
		{
			return ApplyChanges(corrections, true);
		}

		/// <inheritdoc />
		public virtual SyncSession BeginSync(Guid sessionId, SyncOptions options)
		{
			if (SyncSession != null)
			{
				throw new InvalidOperationException("An existing sync session is in progress.");
			}

			SyncSession = new SyncSession { Id = sessionId, StartedOn = TimeService.UtcNow };
			SyncOptions = options;
			Statistics.Reset();

			return SyncSession;
		}

		/// <inheritdoc />
		public virtual SyncStatistics EndSync(Guid sessionId)
		{
			ValidateSession(sessionId);
			SyncSession = null;
			return Statistics;
		}

		/// <inheritdoc />
		public virtual ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request)
		{
			ValidateSession(sessionId);

			var response = new ServiceResult<SyncObject>
			{
				Skipped = request.Skip,
				TotalCount = GetChangeCount(request)
			};

			if (response.TotalCount == 0)
			{
				_changeCount = -1;
				return response;
			}

			// if the since and until are equal that means we should get all changes from since to now
			if (request.Since == request.Until)
			{
				request.Until = TimeService.UtcNow;
			}

			var take = request.Take <= 0 || request.Take > SyncOptions.ItemsPerSyncRequest ? SyncOptions.ItemsPerSyncRequest : request.Take;
			var remainingSkip = request.Skip;
			using var database = _provider.GetSyncableDatabase();

			foreach (var repository in database.GetSyncableRepositories(SyncOptions))
			{
				// Skip this type if it's being filters or if the outgoing converter cannot convert
				if (SyncOptions.ShouldFilterRepository(repository.TypeName)
					|| OutgoingConverter != null && !OutgoingConverter.CanConvert(repository.TypeName))
				{
					// Do not process this repository because we have filters and the repository is not in the filters.
					continue;
				}

				var filter = SyncOptions.GetRepositoryFilter(repository);

				// Check to see if this repository should be skipped
				var changeCount = repository.GetChangeCount(request.Since, request.Until, filter);
				if (changeCount <= remainingSkip)
				{
					// this repo changes was processed in a previous GetChanges request
					remainingSkip -= changeCount;
					continue;
				}

				var changes = repository.GetChanges(request.Since, request.Until, remainingSkip, take - response.Collection.Count, filter).ToList();
				var items = OutgoingConverter?.Convert(changes).ToList() ?? changes;

				response.Collection.AddRange(items);
				remainingSkip = 0;

				if (response.Collection.Count >= take)
				{
					// We have filled up the response so time to return
					break;
				}
			}

			_changeCount = -1;

			Statistics.Changes += response.Collection.Count;

			return response;
		}

		/// <inheritdoc />
		public virtual ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues)
		{
			ValidateSession(sessionId);

			var response = new ServiceResult<SyncObject>();

			if (issues == null)
			{
				return response;
			}

			using var database = _provider.GetSyncableDatabase();

			foreach (var item in issues.Collection)
			{
				var issue = IncomingConverter == null ? item : IncomingConverter.Convert(item);

				switch (issue?.IssueType)
				{
					case SyncIssueType.Unknown:
					case SyncIssueType.ConstraintException:
					default:
						// Do not process these issues
						break;

					case SyncIssueType.RelationshipConstraint:
						var type = Type.GetType(issue.TypeName);

						if (SyncOptions.ShouldFilterRepository(type))
						{
							// Do not process this issue because we have filters and the repository is not in the filters.
							continue;
						}

						// Assuming this is because this entity or a relationship it depends on was deleted but then used 
						// in another client or server. This means we should sync it again.
						var repository = database.GetSyncableRepository(type);
						if (repository == null)
						{
							// todo: How would we communicate this error back to the request?
							continue;
						}

						var entity = repository.Read(issue.Id);
						if (entity == null)
						{
							// todo: How would we communicate this error back to the request?
							break;
						}

						var syncObject = entity.ToSyncObject();

						if (OutgoingConverter != null)
						{
							syncObject = OutgoingConverter.Convert(syncObject);
						}

						if (syncObject != null)
						{
							response.Collection.Add(syncObject);
						}
						break;
				}
			}

			Statistics.Corrections += response.Collection.Count;

			return response;
		}

		/// <inheritdoc />
		public ISyncableDatabase GetDatabase()
		{
			return _provider.GetSyncableDatabase();
		}

		/// <inheritdoc />
		public T GetDatabase<T>() where T : class, ISyncableDatabase
		{
			return (T) _provider.GetSyncableDatabase();
		}

		/// <summary>
		/// Validates the sync session. The SyncSession will be set on BeginSync and cleared on EndSync.
		/// </summary>
		/// <param name="sessionId"> </param>
		protected virtual void ValidateSession(Guid sessionId)
		{
			if (sessionId != SyncSession?.Id)
			{
				throw new InvalidOperationException("The sync session ID is invalid.");
			}
		}

		private ServiceResult<SyncIssue> ApplyChanges(ServiceRequest<SyncObject> changes, bool corrections)
		{
			// The collection is incoming types
			// todo: performance, could we increase performance by going straight to entity, currently we convert to entity then back to sync object
			// The only issue is processing entities individually. If an entity is added to a context then
			// something goes wrong we'll need to disconnect before processing them individually
			var objects = (IncomingConverter?.Convert(changes.Collection) ?? changes.Collection).ToList();
			var groups = objects.Where(x => x != null).GroupBy(x => x.TypeName).OrderBy(x => x.Key);

			if (corrections)
			{
				Statistics.AppliedCorrections += objects.Count;
			}
			else
			{
				Statistics.AppliedChanges += objects.Count;
			}

			if (_provider.Options.SyncOrder.Any())
			{
				var order = _provider.Options.SyncOrder.ToList();
				groups = groups.OrderBy(x => order.IndexOf(x.Key));
			}

			var response = new ServiceResult<SyncIssue> { Collection = new List<SyncIssue>() };

			groups.ForEach(x => ProcessSyncObjects(_provider, x.Where(y => y.Status != SyncObjectStatus.Deleted), response.Collection, corrections));
			groups.Reverse().ForEach(x => ProcessSyncObjects(_provider, x.Where(y => y.Status == SyncObjectStatus.Deleted), response.Collection, corrections));

			return response;
		}

		private int GetChangeCount(SyncRequest request)
		{
			if (_changeCount >= 0)
			{
				return _changeCount;
			}

			using var database = _provider.GetSyncableDatabase();

			_changeCount = database.GetSyncableRepositories(SyncOptions).Sum(repository =>
			{
				// Skip this type if it's being filters or if the outgoing converter cannot convert
				if (SyncOptions.ShouldFilterRepository(repository.TypeName)
					|| OutgoingConverter != null && !OutgoingConverter.CanConvert(repository.TypeName))
				{
					// Do not count this repository because we have filters and the repository is not in the filters.
					return 0;
				}

				var filter = SyncOptions.GetRepositoryFilter(repository);
				return repository.GetChangeCount(request.Since, request.Until, filter);
			});

			return _changeCount;
		}

		private static IEnumerable<Relationship> GetRelationshipConfigurations(ISyncEntity entity)
		{
			var syncEntityType = typeof(ISyncEntity);
			var properties = entity.GetRealType().GetProperties();
			var syncProperties = properties
				.Where(x => syncEntityType.IsAssignableFrom(x.PropertyType))
				.Select(x => new
				{
					EntityPropertyInfo = x,
					EntityIdPropertyInfo = properties.FirstOrDefault(y => y.Name == x.Name + "Id"),
					EntitySyncIdPropertyInfo = properties.FirstOrDefault(y => y.Name == x.Name + "SyncId"),
					Type = x.PropertyType,
					TypeIdPropertyInfo = x.PropertyType.GetProperties().First(p => p.Name == "Id")
				})
				.ToList();

			var response = syncProperties
				.Where(x => x.EntityIdPropertyInfo != null)
				.Where(x => x.EntitySyncIdPropertyInfo != null)
				.Select(x => new Relationship
				{
					EntityPropertyInfo = x.EntityPropertyInfo,
					EntityIdPropertyInfo = x.EntityIdPropertyInfo,
					EntitySyncId = (Guid?) x.EntitySyncIdPropertyInfo.GetValue(entity),
					Type = x.Type,
					TypeIdPropertyInfo = x.TypeIdPropertyInfo
				})
				.ToList();

			return response;
		}

		private void ProcessSyncObject(ProfilerSession session, SyncObject syncObject, ISyncableDatabase database, bool correction)
		{
			Logger.Instance.Write(SyncSession.Id, correction
					? $"Processing sync object correction {syncObject.SyncId}."
					: $"Processing sync object {syncObject.SyncId}.",
				EventLevel.Verbose);

			var syncEntity = syncObject.ToSyncEntity();

			if (syncEntity == null)
			{
				return;
			}

			if (SyncOptions.ShouldFilterRepository(syncObject.TypeName))
			{
				Logger.Instance.Write(SyncSession.Id, "Ignoring this type because this repository is being filtered.", EventLevel.Verbose);
				return;
			}

			if (SyncOptions.ShouldFilterEntity(syncObject.TypeName, syncEntity))
			{
				Logger.Instance.Write(SyncSession.Id, "Ignoring this type because this entity is being filtered.", EventLevel.Verbose);
				return;
			}

			var type = syncEntity.GetType();
			var repository = database.GetSyncableRepository(type);

			if (repository == null)
			{
				throw new InvalidDataException("Failed to find a syncable repository for the entity.");
			}

			var filter = SyncOptions.GetRepositoryFilter(repository);
			var foundEntity = repository.Read(syncEntity, filter);
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
					// Instantiate a new instance of the sync entity to update, also use the provided sync ID
					// this is because it's possibly the sync entity is blocking updating of the sync ID so it 
					// will need to be set manually being that it will be filtered on update.
					foundEntity = (ISyncEntity) Activator.CreateInstance(syncEntity.GetType());
					foundEntity.SyncId = syncObject.SyncId;
					if (!UpdateEntity(syncEntity, foundEntity, syncStatus))
					{
						// todo: should we add a sync issue?
						break;
					}
					UpdateLocalRelationships(foundEntity, database);
					repository.Add(foundEntity);
					break;

				case SyncObjectStatus.Modified:
					if (foundEntity != null && (foundEntity.ModifiedOn < syncEntity.ModifiedOn || correction))
					{
						if (!UpdateEntity(syncEntity, foundEntity, syncStatus))
						{
							// todo: should we add a sync issue?
							break;
						}

						UpdateLocalRelationships(foundEntity, database);
					}
					break;

				case SyncObjectStatus.Deleted:
					if (foundEntity != null)
					{
						if (!UpdateEntity(syncEntity, foundEntity, syncStatus))
						{
							// todo: should we add a sync issue?
							break;
						}

						if (SyncOptions.PermanentDeletions)
						{
							repository.Remove(foundEntity);
						}
						else
						{
							foundEntity.IsDeleted = true;
						}
					}
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void ProcessSyncObjects(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var profileSession = Profiler.Start(() => nameof(ProcessSyncObject));
			var objects = syncObjects.ToList();

			if (!objects.Any())
			{
				profileSession.Stop();
				return;
			}

			try
			{
				using var database = provider.GetSyncableDatabase();
				database.Options.MaintainCreatedOn = false;
				database.Options.MaintainModifiedOn = Options.MaintainModifiedOn;
				for (var i = 0; i < objects.Count; i++)
				{
					ProcessSyncObject(profileSession, objects[i], database, corrections);
				}
				database.SaveChanges();
			}
			catch
			{
				Logger.Instance.Write(SyncSession.Id, "Failed to process sync objects in the batch.");
				ProcessSyncObjectsIndividually(profileSession, provider, objects, issues, corrections);
			}
			finally
			{
				profileSession.Stop();
			}
		}

		private void ProcessSyncObjectsIndividually(ProfilerSession profilerSession, ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var objects = syncObjects.ToList();

			foreach (var syncObject in objects)
			{
				try
				{
					using var database = provider.GetSyncableDatabase();
					database.Options.MaintainCreatedOn = false;
					database.Options.MaintainModifiedOn = Options.MaintainModifiedOn;
					ProcessSyncObject(profilerSession, syncObject, database, corrections);
					database.SaveChanges();
				}
				catch (SyncIssueException ex)
				{
					ex.Issues.ForEach(issues.Add);

					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.RelationshipConstraint,
						Message = "This entity has relationship issue with other entities.",
						TypeName = syncObject.TypeName
					};

					if (SyncOptions.IncludeIssueDetails)
					{
						issue.Message += Environment.NewLine + ex.ToDetailedString();
					}

					issues.Add(issue);
				}
				catch (InvalidConstraintException ex)
				{
					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.ConstraintException,
						Message = "Invalid constraint exception...",
						TypeName = syncObject.TypeName
					};

					if (SyncOptions.IncludeIssueDetails)
					{
						issue.Message += Environment.NewLine + ex.ToDetailedString();
					}

					issues.Add(issue);
				}
				catch (InvalidOperationException ex)
				{
					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.RelationshipConstraint,
						Message = "Invalid operation exception...",
						TypeName = syncObject.TypeName
					};

					if (SyncOptions.IncludeIssueDetails)
					{
						issue.Message += Environment.NewLine + ex.ToDetailedString();
					}

					issues.Add(issue);
				}
				catch (Exception ex)
				{
					var details = ex.ToDetailedString();

					// Cannot catch the DbUpdateException without reference EntityFramework.
					var issue = details.Contains("conflicted with the FOREIGN KEY constraint")
						|| details.Contains("The DELETE statement conflicted with the REFERENCE constraint")
							? new SyncIssue
							{
								Id = syncObject.SyncId,
								IssueType = SyncIssueType.RelationshipConstraint,
								Message = "This entity has relationship issue with another entity.",
								TypeName = syncObject.TypeName
							}
							: new SyncIssue
							{
								Id = syncObject.SyncId,
								IssueType = SyncIssueType.Unknown,
								Message = "Unknown issue...",
								TypeName = syncObject.TypeName
							};

					if (SyncOptions.IncludeIssueDetails)
					{
						issue.Message += Environment.NewLine + ex.ToDetailedString();
					}

					issues.Add(issue);
				}
			}
		}

		private bool UpdateEntity(ISyncEntity source, ISyncEntity destination, SyncObjectStatus status)
		{
			if (IncomingConverter != null)
			{
				return IncomingConverter.Update(source, destination, status);
			}

			if (destination != null && source != null && destination != source)
			{
				destination.UpdateWith(source, false, false, true);
			}

			return true;
		}

		/// <summary>
		/// Updates the entities local relationships.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="database"> The database with the relationship repositories. </param>
		/// <exception cref="SyncIssueException"> An exception will all sync issues. </exception>
		private void UpdateLocalRelationships(ISyncEntity entity, ISyncableDatabase database)
		{
			var response = new List<SyncIssue>();
			var isMemoryDatabase = database is Database;

			foreach (var relationship in GetRelationshipConfigurations(entity))
			{
				if (!relationship.EntitySyncId.HasValue || relationship.EntitySyncId == Guid.Empty)
				{
					continue;
				}

				// Check to see if this is a memory database, these should not be "optimized"
				if (Options.EnablePrimaryKeyCache && !isMemoryDatabase)
				{
					// Only optimize non-memory databases, memory database should always run code below
					var entityId = CacheManager.GetEntityId(relationship.Type, relationship.EntitySyncId.Value);
					if (entityId != null)
					{
						relationship.EntityIdPropertyInfo.SetValue(entity, entityId);
						continue;
					}
				}

				var repository = database.GetSyncableRepository(relationship.Type);
				var foundEntity = repository?.Read(relationship.EntitySyncId.Value);

				if (foundEntity != null)
				{
					var id = relationship.TypeIdPropertyInfo.GetValue(foundEntity);
					relationship.EntityIdPropertyInfo.SetValue(entity, id);

					// if we are a speedy database
					if (isMemoryDatabase)
					{
						// Then we also need to update the actual relationship so the database
						// doesn't reset the relationship IDs thinking the entity has changed
						relationship.EntityPropertyInfo.SetValue(entity, foundEntity);
					}

					if (Options.EnablePrimaryKeyCache)
					{
						CacheManager.CacheEntityId(relationship.Type, relationship.EntitySyncId.Value, id);
					}
					continue;
				}

				response.Add(new SyncIssue
				{
					Id = relationship.EntitySyncId.Value,
					IssueType = SyncIssueType.RelationshipConstraint,
					Message = "Failed to find the entity",
					TypeName = relationship.Type.ToAssemblyName()
				});
			}

			if (response.Any(x => x != null))
			{
				throw new SyncIssueException("This entity has relationship issues.", response.Where(x => x != null));
			}
		}

		#endregion
	}
}