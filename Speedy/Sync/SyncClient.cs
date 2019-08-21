#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Speedy.Configuration;
using Speedy.Exceptions;
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
		private SyncSession _session;

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
		public SyncClientConverter IncomingConverter { get; set; }

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public SyncClientOptions Options { get; set; }

		/// <inheritdoc />
		public SyncClientConverter OutgoingConverter { get; set; }

		/// <inheritdoc />
		public SyncStatistics Statistics { get; set; }

		/// <inheritdoc />
		public SyncOptions SyncOptions { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public ServiceResult<SyncIssue> ApplyChanges(Guid sessionId, ServiceRequest<SyncObject> changes)
		{
			return ApplyChanges(changes, false);
		}

		/// <inheritdoc />
		public ServiceResult<SyncIssue> ApplyCorrections(Guid sessionId, ServiceRequest<SyncObject> corrections)
		{
			return ApplyChanges(corrections, true);
		}

		/// <inheritdoc />
		public SyncSession BeginSync(Guid sessionId, SyncOptions options)
		{
			_session = new SyncSession { Id = sessionId, StartedOn = TimeService.UtcNow };

			SyncOptions = options;
			Statistics.Reset();

			return _session;
		}

		/// <inheritdoc />
		public void EndSync(SyncSession session)
		{
		}

		/// <inheritdoc />
		public ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request)
		{
			var currentSkippedCount = 0;
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

			using (var database = _provider.GetSyncableDatabase())
			{
				foreach (var repository in database.GetSyncableRepositories(SyncOptions))
				{
					var filter = SyncOptions.GetRepositoryFilter(repository);
					var changeCount = repository.GetChangeCount(request.Since, request.Until, filter);

					if (changeCount + currentSkippedCount <= request.Skip)
					{
						currentSkippedCount += changeCount;
						continue;
					}

					var changes = repository.GetChanges(request.Since, request.Until, request.Skip - currentSkippedCount, SyncOptions.ItemsPerSyncRequest - currentSkippedCount, filter).ToList();
					var items = OutgoingConverter != null ? OutgoingConverter.Process(changes).ToList() : changes;
					response.Collection.AddRange(items);
					currentSkippedCount += items.Count;

					if (response.Collection.Count >= take)
					{
						break;
					}
				}

				_changeCount = -1;

				Statistics.Changes += response.Collection.Count;

				return response;
			}
		}

		/// <inheritdoc />
		public ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues)
		{
			var response = new ServiceResult<SyncObject>();

			if (issues == null)
			{
				return response;
			}

			using (var database = _provider.GetSyncableDatabase())
			{
				foreach (var item in issues.Collection)
				{
					var issue = IncomingConverter == null ? item : IncomingConverter.Process(item);

					switch (issue?.IssueType)
					{
						case SyncIssueType.Unknown:
						case SyncIssueType.ConstraintException:
						default:
							// Do not process these issues
							break;

						case SyncIssueType.RelationshipConstraint:
							var type = Type.GetType(issue.TypeName);

							if (SyncOptions?.ShouldFilterRepository(type) == true)
							{
								// Do not process this issue because the repository is not in the filter.
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
								syncObject = OutgoingConverter.Process(syncObject);
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

		/// <inheritdoc />
		public void UpdateOptions(Guid id, SyncClientOptions options)
		{
			Options.UpdateWith(options);
		}

		private ServiceResult<SyncIssue> ApplyChanges(ServiceRequest<SyncObject> changes, bool corrections)
		{
			var objects = (IncomingConverter?.Process(changes.Collection) ?? changes.Collection).ToList();
			var groups = objects.GroupBy(x => x.TypeName).OrderBy(x => x.Key);

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

			using (var database = _provider.GetSyncableDatabase())
			{
				_changeCount = database.GetSyncableRepositories(SyncOptions).Sum(repository =>
				{
					var filter = SyncOptions.GetRepositoryFilter(repository);
					return repository.GetChangeCount(request.Since, request.Until, filter);
				});

				return _changeCount;
			}
		}

		private static IEnumerable<Relationship> GetRelationshipConfigurations(ISyncEntity entity)
		{
			var syncEntityType = typeof(ISyncEntity);
			var properties = entity.GetRealType().GetProperties();
			var syncProperties = properties
				.Where(x => syncEntityType.IsAssignableFrom(x.PropertyType))
				.Select(x => new
				{
					IdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "Id"),
					SyncIdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "SyncId"),
					Type = x.PropertyType,
					TypeIdPropertyInfo = x.PropertyType.GetProperties().First(p => p.Name == "Id")
				})
				.ToList();

			var response = syncProperties
				.Where(x => x.IdProperty != null)
				.Where(x => x.SyncIdProperty != null)
				.Select(x => new Relationship
				{
					IdPropertyInfo = x.IdProperty,
					SyncId = (Guid?) x.SyncIdProperty.GetValue(entity),
					Type = x.Type,
					TypeIdPropertyInfo = x.TypeIdPropertyInfo
				})
				.ToList();

			return response;
		}

		private void ProcessSyncObject(SyncObject syncObject, ISyncableDatabase database, bool correction)
		{
			Logger.Instance.Write(_session.Id, correction
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
				Logger.Instance.Write(_session.Id, "Ignoring this type because this repository is being filtered.", EventLevel.Verbose);
				return;
			}

			if (SyncOptions.ShouldFilterEntity(syncObject.TypeName, syncEntity))
			{
				Logger.Instance.Write(_session.Id, "Ignoring this type because this entity is being filtered.", EventLevel.Verbose);
				return;
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
					syncEntity.ResetId();
					UpdateLocalRelationships(syncEntity, database);
					repository.Add(syncEntity);
					break;

				case SyncObjectStatus.Modified:
					if (foundEntity != null)
					{
						if (foundEntity.ModifiedOn < syncEntity.ModifiedOn || correction)
						{
							UpdateLocalRelationships(foundEntity, database);
							foundEntity.UpdateWith(syncEntity);
						}
					}
					break;

				case SyncObjectStatus.Deleted:
					if (foundEntity != null)
					{
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
			var syncObjectList = syncObjects.ToList();

			if (!syncObjectList.Any())
			{
				return;
			}

			try
			{
				using (var database = provider.GetSyncableDatabase())
				{
					database.Options.MaintainCreatedOn = false;
					database.Options.MaintainModifiedOn = Options.MaintainModifiedOn;
					syncObjectList.ForEach(x => ProcessSyncObject(x, database, corrections));
					database.SaveChanges();
				}
			}
			catch
			{
				Logger.Instance.Write(_session.Id, "Failed to process sync objects in the batch.");
				ProcessSyncObjectsIndividually(provider, syncObjectList, issues, corrections);
			}
		}

		private void ProcessSyncObjectsIndividually(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			foreach (var syncObject in syncObjects)
			{
				try
				{
					using (var database = provider.GetSyncableDatabase())
					{
						database.Options.MaintainCreatedOn = false;
						database.Options.MaintainModifiedOn = Options.MaintainModifiedOn;
						ProcessSyncObject(syncObject, database, corrections);
						database.SaveChanges();
					}
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

		/// <summary>
		/// Updates the entities local relationships.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="database"> The database with the relationship repositories. </param>
		/// <exception cref="SyncIssueException"> An exception will all sync issues. </exception>
		private static void UpdateLocalRelationships(ISyncEntity entity, ISyncableDatabase database)
		{
			var response = new List<SyncIssue>();

			foreach (var relationship in GetRelationshipConfigurations(entity))
			{
				if (!relationship.SyncId.HasValue || relationship.SyncId == Guid.Empty)
				{
					continue;
				}

				var foundEntity = database.GetSyncableRepository(relationship.Type)?.Read(relationship.SyncId.Value);
				if (foundEntity != null)
				{
					var id = relationship.TypeIdPropertyInfo.GetValue(foundEntity);
					relationship.IdPropertyInfo.SetValue(entity, id);
					continue;
				}

				response.Add(new SyncIssue
				{
					Id = relationship.SyncId.Value,
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