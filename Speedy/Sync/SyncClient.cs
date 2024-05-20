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
		private SyncSession _syncSession;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync client.
		/// </summary>
		public SyncClient(string name, ISyncableDatabaseProvider provider)
		{
		_changeCount = -1;

		DatabaseProvider = provider;
		Name = name;
		Options = new SyncClientOptions();
		Profiler = new SyncClientProfiler(name);
		Statistics = new SyncStatistics();
		SyncOptions = new SyncOptions();
	}

		#endregion

		#region Properties

		/// <inheritdoc />
		public ISyncableDatabaseProvider DatabaseProvider { get; }

		/// <inheritdoc />
		public SyncClientIncomingConverter IncomingConverter { get; set; }

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public SyncClientOptions Options { get; protected set; }

		/// <inheritdoc />
		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		/// <inheritdoc />
		public SyncClientProfiler Profiler { get; }

		/// <inheritdoc />
		public SyncStatistics Statistics { get; }

		/// <inheritdoc />
		public SyncOptions SyncOptions { get; protected set; }

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
			if (_syncSession != null)
			{
				throw new InvalidOperationException("An existing sync session is in progress.");
			}

			_syncSession = new SyncSession { Id = sessionId, StartedOn = TimeService.UtcNow };

			SyncOptions = options;
			Statistics.Reset();

			return _syncSession;
		}

		/// <inheritdoc />
		public virtual SyncStatistics EndSync(Guid sessionId)
		{
			ValidateSession(sessionId);
			_syncSession = null;
			return Statistics;
		}

		/// <inheritdoc />
		public virtual ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request)
		{
			ValidateSession(sessionId);

			var response = new ServiceResult<SyncObject>
			{
				Skipped = request.Skip,
				TotalCount = Profiler.GetChangeCount.Time(() => GetChangeCount(request))
			};

			return Profiler.GetChanges.Time(() =>
			{
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

				var take = (request.Take <= 0) || (request.Take > SyncOptions.ItemsPerSyncRequest) ? SyncOptions.ItemsPerSyncRequest : request.Take;
				var remainingSkip = request.Skip;
				using var database = DatabaseProvider.GetSyncableDatabase();

				foreach (var repository in database.GetSyncableRepositories(SyncOptions))
				{
					// Skip this type if it's being filters or if the outgoing converter cannot convert
					if (SyncOptions.ShouldExcludeRepository(repository.TypeName)
						|| ((OutgoingConverter != null) && !OutgoingConverter.CanConvert(repository.TypeName)))
					{
						// Do not process this repository because we have filters and the repository is not in the filters.
						continue;
					}

					var syncRepositoryFilter = SyncOptions.GetFilter(repository);

					// Check to see if this repository should be skipped
					var changeCount = repository.GetChangeCount(request.Since, request.Until, syncRepositoryFilter);
					if (changeCount <= remainingSkip)
					{
						// this repo changes was processed in a previous GetChanges request
						remainingSkip -= changeCount;
						continue;
					}

					var changes = repository.GetChanges(request.Since, request.Until, remainingSkip, take - response.Collection.Count, syncRepositoryFilter).ToList();
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
			});
		}

		/// <inheritdoc />
		public virtual ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues)
		{
			ValidateSession(sessionId);

			var response = new ServiceResult<SyncObject>();

			Statistics.Corrections += response.Collection.Count;

			return response;
		}

		/// <inheritdoc />
		public ISyncableDatabase GetDatabase()
		{
			return DatabaseProvider.GetSyncableDatabase();
		}

		/// <inheritdoc />
		public T GetDatabase<T>() where T : class, ISyncableDatabase
		{
			return (T) DatabaseProvider.GetSyncableDatabase();
		}

		/// <summary>
		/// Validates the sync session. The SyncSession will be set on BeginSync and cleared on EndSync.
		/// </summary>
		/// <param name="sessionId"> </param>
		protected virtual void ValidateSession(Guid sessionId)
		{
			if (sessionId != _syncSession?.Id)
			{
				throw new InvalidOperationException("The sync session ID is invalid.");
			}
		}

		private ServiceResult<SyncIssue> ApplyChanges(ServiceRequest<SyncObject> changes, bool corrections)
		{
			return Profiler.ApplyChanges.Time(() =>
			{
				// The collection is incoming types
				// todo: performance, could we increase performance by going straight to entity, currently we convert to entity then back to sync object
				// The only issue is processing entities individually. If an entity is added to a context then
				// something goes wrong we'll need to disconnect before processing them individually
				var objects = (IncomingConverter?.Convert(changes.Collection) ?? changes.Collection).ToList();
				var groups = objects.Where(x => !x.Equals(SyncObjectExtensions.Empty)).GroupBy(x => x.TypeName).OrderBy(x => x.Key);

				if (corrections)
				{
					Statistics.AppliedCorrections += objects.Count;
				}
				else
				{
					Statistics.AppliedChanges += objects.Count;
				}

				if (DatabaseProvider.Options.SyncOrder.Any())
				{
					var order = DatabaseProvider.Options.SyncOrder.ToList();
					groups = groups.OrderBy(x => order.IndexOf(x.Key));
				}

				var response = new ServiceResult<SyncIssue> { Collection = new List<SyncIssue>() };
				groups.ForEach(x => ProcessSyncObjects(DatabaseProvider, x.Where(y => y.Status != SyncObjectStatus.Deleted), response.Collection, corrections));
				groups.Reverse().ForEach(x => ProcessSyncObjects(DatabaseProvider, x.Where(y => y.Status == SyncObjectStatus.Deleted), response.Collection, corrections));
				response.TotalCount = response.Collection.Count;
				return response;
			});
		}

		private int GetChangeCount(SyncRequest request)
		{
			if (_changeCount >= 0)
			{
				return _changeCount;
			}

			using var database = DatabaseProvider.GetSyncableDatabase();

			_changeCount = database.GetSyncableRepositories(SyncOptions).Sum(repository =>
			{
				// Skip this type if it's being filters or if the outgoing converter cannot convert
				if (SyncOptions.ShouldExcludeRepository(repository.TypeName)
					|| ((OutgoingConverter != null) && !OutgoingConverter.CanConvert(repository.TypeName)))
				{
					// Do not count this repository because we have filters and the repository is not in the filters.
					return 0;
				}

				var syncRepositoryFilter = SyncOptions.GetFilter(repository);
				return repository.GetChangeCount(request.Since, request.Until, syncRepositoryFilter);
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
					EntityIdPropertyInfo = properties.FirstOrDefault(y => y.Name == (x.Name + "Id")),
					EntitySyncIdPropertyInfo = properties.FirstOrDefault(y => y.Name == (x.Name + "SyncId")),
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

		private void ProcessSyncObject(SyncObject syncObject, ISyncableDatabase database, ICollection<SyncIssue> issues, bool correction, bool isIndividualProcess)
		{
			Profiler.ProcessSyncObject.Time(() =>
			{
				Logger.Instance.Write(_syncSession?.Id ?? Guid.Empty, correction
						? $"Processing sync object {syncObject.TypeName} {syncObject.SyncId} correction."
						: $"Processing sync object {syncObject.TypeName} {syncObject.SyncId}.",
					EventLevel.Verbose);

				if (SyncOptions.ShouldExcludeRepository(syncObject.TypeName))
				{
					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.RepositoryFiltered,
						Message = "The item is not being processed because this repository is being filtered.",
						TypeName = syncObject.TypeName
					};
					issues.Add(issue);
					Logger.Instance.Write(_syncSession?.Id ?? Guid.Empty, issue.Message, EventLevel.Verbose);
					return;
				}

				var syncEntity = syncObject.ToSyncEntity();

				if (syncEntity == null)
				{
					return;
				}

				if (SyncOptions.ShouldFilterIncomingEntity(syncObject.TypeName, syncEntity))
				{
					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.SyncEntityFiltered,
						Message = "The item is not being processed because the sync entity is being filtered.",
						TypeName = syncObject.TypeName
					};
					issues.Add(issue);
					Logger.Instance.Write(_syncSession?.Id ?? Guid.Empty, issue.Message, EventLevel.Verbose);
					return;
				}

				var type = syncEntity.GetType();
				var repository = database.GetSyncableRepository(type);

				if (repository == null)
				{
					throw new InvalidDataException("Failed to find a syncable repository for the entity.");
				}

				var syncRepositoryFilter = SyncOptions.GetFilter(repository);
				var foundEntity = Profiler.ProcessSyncObjectReadEntity.Time(() =>
				{
					// Check to see if primary key caching is enabled and is never expiring for a client
					// This combination of state means we are caching all keys for a local client to reduce
					// the amount of database access.
					//
					// NOTE: This means the database MUST cache all primary keys as they are stored. If the
					// database fails to update the cache manager then this would result in processing of
					// sync items individually which could destroy performance.
					//
					// Disable caching when running "individual" processing just in case there is caching issues.
					// Disable caching if the repository is using a different lookup filter because matching could be using a different "sync lookup key"
					//  - todo: change key cache to add a "GetEntitySyncId" (see GetEntityId) method, this way we could cache on any lookup key
					// Disable caching if the cache does not support the sync entity type
					var doesNotHaveLookupFilter = syncRepositoryFilter?.HasLookupFilter != true;
					if (doesNotHaveLookupFilter
						&& !isIndividualProcess
						&& Options.EnablePrimaryKeyCache
						&& !Options.IsServerClient
						&& (database.KeyCache?.SupportsType(type) == true))
					{
						var id = database.KeyCache.GetEntityId(syncEntity);
						if (id == null)
						{
							// The ID was not found so the entity is to believed to not exist.
							return null;
						}

						// Id was found so let's read the entity by the primary key
						var readEntity = repository.ReadByPrimaryId(id);
						if ((readEntity != null) && (readEntity.GetEntitySyncId() == syncEntity.SyncId))
						{
							// The entity was found so return it by ID.
							return readEntity;
						}
					}

					return doesNotHaveLookupFilter
						? repository.Read(syncObject.SyncId)
						: repository.Read(syncEntity, syncRepositoryFilter);
				});

				var syncStatus = syncObject.Status;

				if ((foundEntity != null) && (syncObject.Status == SyncObjectStatus.Added))
				{
					syncStatus = SyncObjectStatus.Modified;
				}
				else if ((foundEntity == null) && (syncObject.Status == SyncObjectStatus.Modified))
				{
					syncStatus = SyncObjectStatus.Added;
				}

				if (syncEntity.IsDeleted && (syncStatus != SyncObjectStatus.Deleted))
				{
					syncStatus = SyncObjectStatus.Deleted;
				}

				switch (syncStatus)
				{
					case SyncObjectStatus.Added:
					{
						Profiler.ProcessSyncObjectAdded.Time(() =>
						{
							// Instantiate a new instance of the sync entity to update, also use the provided sync ID
							// this is because it's possibly the sync entity is blocking updating of the sync ID so it 
							// will need to be set manually being that it will be filtered on update.
							foundEntity = (ISyncEntity) Activator.CreateInstance(syncEntity.GetType());
							if (foundEntity == null)
							{
								throw new SyncIssueException(SyncIssueType.Unknown, "Failed to create a new instance.");
							}
							foundEntity.SyncId = syncObject.SyncId;

							if (UpdateEntity(database, syncObject, syncEntity, foundEntity, syncStatus, issues))
							{
								repository.Add(foundEntity);
							}
						});
						break;
					}
					case SyncObjectStatus.Modified:
					{
						Profiler.ProcessSyncObjectModified.Time(() =>
						{
							if ((foundEntity == null) || ((foundEntity.ModifiedOn >= syncEntity.ModifiedOn) && !correction))
							{
								// Did not find the entity or it has not changed.
								return;
							}

							if (!UpdateEntity(database, syncObject, syncEntity, foundEntity, syncStatus, issues))
							{
								// todo: roll back any possible changes
								//database.RevertChanges(foundEntity);
							}
						});
						break;
					}
					case SyncObjectStatus.Deleted:
					{
						Profiler.ProcessSyncObjectDeleted.Time(() =>
						{
							var entityNotFound = foundEntity == null;
							if (entityNotFound)
							{
								// Check to see if we are permanently deleting sync entity
								if (SyncOptions.PermanentDeletions)
								{
									// Entity not found and we don't soft delete so bounce
									return;
								}

								// We did not find the entity and we should be soft deleting
								// this means we must "add" the entity so we can delete it

								// Insert the "soft deleted" item into the database "IsDeleted" will be handled below.
								foundEntity = (ISyncEntity) Activator.CreateInstance(syncEntity.GetType());
								foundEntity.SyncId = syncObject.SyncId;
							}

							if (!UpdateEntity(database, syncObject, syncEntity, foundEntity, syncStatus, issues))
							{
								// todo: roll back any possible changes
								return;
							}

							if (SyncOptions.PermanentDeletions)
							{
								repository.Remove(foundEntity);
								return;
							}

							foundEntity.IsDeleted = true;

							if (entityNotFound)
							{
								repository.Add(foundEntity);
							}
						});
						break;
					}
					default:
					{
						throw new ArgumentOutOfRangeException();
					}
				}
			});
		}

		private void ProcessSyncObjects(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var objects = Profiler.ProcessSyncObjectsSyncObjectsToList.Time(syncObjects.ToList);
			if (objects.Count <= 0)
			{
				return;
			}

			try
			{
				var database = Profiler.ProcessSyncObjectsGetDatabase.Time(() =>
				{
					var d = provider.GetSyncableDatabase();
					d.Options.MaintainCreatedOn = false;
					d.Options.MaintainModifiedOn = Options.IsServerClient;
					return d;
				});

				try
				{
					Profiler.ProcessSyncObjects.Time(() =>
					{
						for (var i = 0; i < objects.Count; i++)
						{
							ProcessSyncObject(objects[i], database, issues, corrections, false);
						}
					});

					Profiler.ProcessSyncObjectsSaveDatabase.Time(() => database.SaveChanges());
				}
				finally
				{
					database.Dispose();
				}
			}
			catch
			{
				Statistics.IndividualProcessCount++;
				Logger.Instance.Write(_syncSession?.Id ?? Guid.Empty, "Failed to process sync objects in the batch.", EventLevel.Warning);
				ProcessSyncObjectsIndividually(provider, objects, issues, corrections);
			}
		}

		private void ProcessSyncObjectsIndividually(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var objects = syncObjects.ToList();

			foreach (var syncObject in objects)
			{
				try
				{
					using var database = provider.GetSyncableDatabase();
					database.Options.MaintainCreatedOn = false;
					database.Options.MaintainModifiedOn = Options.IsServerClient;
					ProcessSyncObject(syncObject, database, issues, corrections, true);
					database.SaveChanges();
				}
				catch (SyncIssueException ex)
				{
					ex.Issues.ForEach(issues.Add);

					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = ex.IssueType,
						Message = ex.Message,
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
				catch (ValidationException ex)
				{
					var issue = new SyncIssue
					{
						Id = syncObject.SyncId,
						IssueType = SyncIssueType.ValidationException,
						Message = ex.Message,
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

		private bool UpdateEntity(ISyncableDatabase database, SyncObject syncObject, ISyncEntity syncEntity, ISyncEntity foundEntity, SyncObjectStatus syncStatus, ICollection<SyncIssue> issues)
		{
			try
			{
				if (!UpdateEntity(syncEntity, foundEntity, syncStatus))
				{
					// returning false just means do not process and do not return a sync issue
					return false;
				}

				UpdateLocalRelationships(foundEntity, database);
				return true;
			}
			catch (UpdateException ex)
			{
				// throwing an update exception just means return a sync issue
				var issue = new SyncIssue
				{
					Id = syncObject.SyncId,
					IssueType = SyncIssueType.UpdateException,
					Message = ex.Message,
					TypeName = syncObject.TypeName
				};
				issues.Add(issue);
				return false;
			}
		}

		private bool UpdateEntity(ISyncEntity source, ISyncEntity destination, SyncObjectStatus status)
		{
			if (IncomingConverter != null)
			{
				return IncomingConverter.Update(source, destination, status);
			}

			if ((destination != null)
				&& (source != null)
				&& !Equals(destination, source))
			{
				destination.UpdateSyncEntity(source, false, false, true);
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
				if (!relationship.EntitySyncId.HasValue || (relationship.EntitySyncId == Guid.Empty))
				{
					continue;
				}

				if (Options.EnablePrimaryKeyCache)
				{
					var entityId = database.KeyCache?.GetEntityId(relationship.Type, relationship.EntitySyncId.Value);
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
						database.KeyCache?.AddEntityId(relationship.Type, relationship.EntitySyncId.Value, id);
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
				throw new SyncIssueException(SyncIssueType.RelationshipConstraint,
					"This entity has relationship issues.",
					response.Where(x => x != null).ToArray());
			}
		}

		#endregion
	}
}