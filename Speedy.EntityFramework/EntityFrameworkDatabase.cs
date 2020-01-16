#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Speedy.Exceptions;
using Speedy.Storage;
using Speedy.Sync;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents an Entity Framework Speedy database.
	/// </summary>
	public abstract class EntityFrameworkDatabase : DbContext, ISyncableDatabase
	{
		#region Fields

		private readonly CollectionChangeTracker _collectionChangeTracker;
		private int _saveChangeCount;
		private readonly ConcurrentDictionary<string, ISyncableRepository> _syncableRepositories;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database.
		/// </summary>
		protected EntityFrameworkDatabase()
		{
			DbContextOptions = null;
			Options = new DatabaseOptions();

			_collectionChangeTracker = new CollectionChangeTracker();
			_syncableRepositories = new ConcurrentDictionary<string, ISyncableRepository>();

			if (Options.DetectSyncableRepositories)
			{
				DetectSyncableRepositories();
			}
		}

		/// <summary>
		/// Instantiates an instance of the database.
		/// </summary>
		/// <param name="startup"> The startup options for this database. </param>
		/// <param name="options"> The options for this database. </param>
		protected EntityFrameworkDatabase(DbContextOptions startup, DatabaseOptions options)
			: base(startup)
		{
			DbContextOptions = startup;
			Options = options ?? new DatabaseOptions();

			_collectionChangeTracker = new CollectionChangeTracker();
			_syncableRepositories = new ConcurrentDictionary<string, ISyncableRepository>();

			if (Options.DetectSyncableRepositories)
			{
				DetectSyncableRepositories();
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the database context options for this database.
		/// </summary>
		public DbContextOptions DbContextOptions { get; }

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public T Add<T, T2>(T item) where T : Entity<T2>
		{
			GetRepository<T, T2>().Add(item);
			return item;
		}

		/// <summary>
		/// Discard all changes made in this context to the underlying database.
		/// </summary>
		public int DiscardChanges()
		{
			var count = 0;

			foreach (var entry in ChangeTracker.Entries().ToList())
			{
				switch (entry.State)
				{
					case EntityState.Modified:
						entry.CurrentValues.SetValues(entry.OriginalValues);
						entry.State = EntityState.Unchanged;
						count++;
						break;

					case EntityState.Deleted:
						entry.State = EntityState.Unchanged;
						count++;
						break;

					case EntityState.Added:
						entry.State = EntityState.Detached;
						count++;
						break;
				}
			}

			return count;
		}

		/// <inheritdoc />
		public IRepository<T, T2> GetReadOnlyRepository<T, T2>() where T : Entity<T2>
		{
			return new ReadOnlyEntityFrameworkRepository<T, T2>(this, Set<T>());
		}

		/// <inheritdoc />
		public IRepository<T, T2> GetRepository<T, T2>() where T : Entity<T2>
		{
			return new EntityFrameworkRepository<T, T2>(this, Set<T>());
		}

		/// <inheritdoc />
		public IEnumerable<ISyncableRepository> GetSyncableRepositories(SyncOptions options)
		{
			if (_syncableRepositories.Count <= 0)
			{
				// Refresh the syncable repositories
				DetectSyncableRepositories();
			}

			if (Options.SyncOrder.Length <= 0)
			{
				return _syncableRepositories
					.Values
					.Where(x => !options.ShouldFilterRepository(x.TypeName))
					.ToList();
			}

			var order = Options.SyncOrder.Reverse().ToList();
			var ordered = _syncableRepositories
				.Where(x => !options.ShouldFilterRepository(x.Key))
				.OrderBy(x => x.Key == order[0]);

			var response = order
				.Skip(1)
				.Aggregate(ordered, (current, key) => current.ThenBy(x => x.Key == key))
				.Select(x => x.Value)
				.ToList();

			return response;
		}

		/// <inheritdoc />
		public ISyncableRepository<T, T2> GetSyncableRepository<T, T2>() where T : SyncEntity<T2>
		{
			return new EntityFrameworkSyncableRepository<T, T2>(this, Set<T>());
		}

		/// <summary>
		/// Gets a syncable repository of the requested entity.
		/// </summary>
		/// <returns> The repository of entities requested. </returns>
		public ISyncableRepository GetSyncableRepository(Type type)
		{
			if (_syncableRepositories.TryGetValue(type.ToAssemblyName(), out var repository))
			{
				return repository;
			}

			var idType = type.GetCachedProperties(BindingFlags.Public | BindingFlags.Instance).First(x => x.Name == "Id").PropertyType;
			var methods = GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
			var setMethod = methods.First(x => x.Name == "Set" && x.IsGenericMethodDefinition);
			var method = setMethod.MakeGenericMethod(type);
			var entitySet = method.Invoke(this, null);
			var repositoryType = typeof(EntityFrameworkSyncableRepository<,>).MakeGenericType(type, idType);
			repository = Activator.CreateInstance(repositoryType, this, entitySet) as ISyncableRepository;

			_syncableRepositories.AddOrUpdate(type.ToAssemblyName(), repository, (k, v) => repository);

			return repository;
		}

		/// <summary>
		/// Saves all changes made in this context to the underlying database.
		/// </summary>
		/// <returns>
		/// The number of objects written to the underlying database.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException"> Thrown if the context has been disposed. </exception>
		public override int SaveChanges()
		{
			if (_saveChangeCount++ > 2)
			{
				throw new OverflowException("Database save changes stuck in a processing loop.");
			}

			try
			{
				var first = _saveChangeCount == 1;
				if (first)
				{
					_collectionChangeTracker.Reset();
				}

				ChangeTracker.Entries().ToList().ForEach(ProcessEntity);

				// The local relationships may have changed. We need keep our sync IDs in sync with any relationships that may have changed.
				ChangeTracker.Entries().ToList().ForEach(x => (x.Entity as ISyncEntity)?.UpdateLocalSyncIds());

				var response = base.SaveChanges();
				var needsMoreSaving = ChangeTracker.Entries().Any(x => x.State != EntityState.Detached && x.State != EntityState.Unchanged);

				if (needsMoreSaving)
				{
					response += SaveChanges();
				}

				if (first)
				{
					OnCollectionChanged(_collectionChangeTracker.Added, _collectionChangeTracker.Removed);
				}

				return response;
			}
			catch (Exception ex)
			{
				ProcessException(ex);
				throw;
			}
			finally
			{
				_saveChangeCount = 0;
			}
		}

		/// <summary>
		/// Gets the assembly that contains the entity mappings. Base implementation defaults to the implemented types assembly.
		/// </summary>
		protected virtual Assembly GetMappingAssembly()
		{
			return GetType().Assembly;
		}

		/// <summary>
		/// This method is called when the model for a derived context has been initialized, but before the model has been locked
		/// down and used to initialize
		/// the context. The default implementation of this method does nothing, but it can be overridden in a derived class such
		/// that the model can be
		/// further configured before it is locked down.
		/// </summary>
		/// <remarks>
		/// Typically, this method is called only once when the first instance of a derived context is created. The model for that
		/// context is then cached and
		/// is for all further instances of the context in the app domain. This caching can be disabled by setting the ModelCaching
		/// property on the given
		/// ModelBuilder, but note that this can seriously degrade performance. More control over caching is provided through use
		/// of the DbModelBuilder and
		/// DbContextFactory classes directly.
		/// </remarks>
		/// <param name="modelBuilder"> The builder that defines the model for the context being created. </param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			var assembly = GetMappingAssembly();
			var types = assembly.GetTypes();
			var mappingTypes = types.Where(x => !x.IsAbstract && x.GetInterfaces().Any(y => y == typeof(IEntityMappingConfiguration)));

			foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
			{
				config.Map(modelBuilder);
			}

			ProcessModelTypes(modelBuilder);

			base.OnModelCreating(modelBuilder);
		}

		/// <summary>
		/// Allows data context a chance to process the exception that occurred during save changes.
		/// </summary>
		/// <param name="exception"> The exception that occurred when trying to save changes. </param>
		protected virtual void ProcessException(Exception exception)
		{
			switch (exception)
			{
				case ValidationException ve:
					throw new Exceptions.ValidationException(ve.Message);

				case DbUpdateException ue:
					// Wrap in a Speedy exception for consistency
					throw new UpdateException(ue.Message, ue);
			}
		}

		/// <summary>
		/// Processes the model builder for the Speedy default types.
		/// </summary>
		/// <remarks>
		/// DateTime values will default to "datetime2".
		/// Guid values default to "uniqueidentifier" type.
		/// Strings default to non-unicode.
		/// </remarks>
		/// <param name="modelBuilder"> </param>
		protected virtual void ProcessModelTypes(ModelBuilder modelBuilder)
		{
			foreach (var entity in modelBuilder.Model.GetEntityTypes())
			{
				var properties = entity.GetProperties();

				foreach (var p in properties)
				{
					switch (p.ClrType)
					{
						case Type _ when p.ClrType == typeof(DateTime):
						case Type _ when p.ClrType == typeof(DateTime?):
							p.SetColumnType("datetime2");
							break;

						case Type _ when p.ClrType == typeof(Guid):
						case Type _ when p.ClrType == typeof(Guid?):
							p.SetColumnType("uniqueidentifier");
							break;

						case Type _ when p.ClrType == typeof(string):
							p.SetIsUnicode(false);
							break;
					}
				}
			}
		}

		/// <summary>
		/// Reads all repositories and puts all the syncable ones in an internal list.
		/// </summary>
		private void DetectSyncableRepositories()
		{
			var type = GetType();
			var test = type.GetCachedProperties();
			var properties = test.Where(x => x.PropertyType.Name == typeof(IRepository<,>).Name || x.PropertyType.Name == typeof(ISyncableRepository<,>).Name).ToList();

			_syncableRepositories.Clear();
			var syncEntityType = typeof(ISyncEntity);

			foreach (var property in properties)
			{
				var genericType = property.PropertyType.GetGenericArguments().First();

				if (!syncEntityType.IsAssignableFrom(genericType))
				{
					continue;
				}

				GetSyncableRepository(genericType);
			}
		}

		private void OnCollectionChanged(IList added, IList removed)
		{
			if (CollectionChanged == null)
			{
				return;
			}

			NotifyCollectionChangedEventArgs eventArgs = null;

			if (added.Count > 0 && removed.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, added, removed);
			}
			else if (added.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added);
			}
			else if (removed.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed);
			}

			if (eventArgs != null)
			{
				CollectionChanged?.Invoke(this, eventArgs);
			}
		}

		/// <summary>
		/// Manages the created on and modified on members of the base entity.
		/// </summary>
		/// <param name="entry"> </param>
		private void ProcessEntity(EntityEntry entry)
		{
			var states = new[] { EntityState.Added, EntityState.Deleted, EntityState.Modified };
			if (!states.Contains(entry.State))
			{
				// This entity is a state that is not tracked.
				return;
			}

			// entry should be of type IEntity, not sure you can get here if not?
			if (!(entry.Entity is IEntity entity))
			{
				return;
			}

			var createdEntity = entity as ICreatedEntity;
			var modifiableEntity = entity as IModifiableEntity;
			var syncableEntity = entity as ISyncEntity;
			var maintainedEntity = Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
			var maintainCreatedOnDate = maintainedEntity && Options.MaintainCreatedOn;
			var maintainModifiedOnDate = maintainedEntity && Options.MaintainModifiedOn;
			var maintainSyncId = maintainedEntity && Options.MaintainSyncId;
			var now = TimeService.UtcNow;

			// Check to see if the entity was added.
			switch (entry.State)
			{
				case EntityState.Added:
					_collectionChangeTracker.Add(entity);

					if (createdEntity != null && maintainCreatedOnDate)
					{
						// Make sure the modified on value matches created on for new items.
						createdEntity.CreatedOn = now;
					}

					if (syncableEntity != null)
					{
						if (maintainSyncId && syncableEntity.SyncId == Guid.Empty)
						{
							syncableEntity.SyncId = Guid.NewGuid();
						}
					}

					if (modifiableEntity != null && maintainModifiedOnDate)
					{
						modifiableEntity.ModifiedOn = now;
					}
					entity.EntityAdded();
					break;

				case EntityState.Modified:
					if (!entity.CanBeModified())
					{
						// Tell entity framework to not update the entity.
						entry.State = EntityState.Unchanged;
						break;
					}

					if (createdEntity != null && maintainCreatedOnDate && entry.CurrentValues.Properties.Any(x => x.Name == nameof(ICreatedEntity.CreatedOn)))
					{
						// Do not allow created on to change for entities.
						createdEntity.CreatedOn = (DateTime) entry.OriginalValues["CreatedOn"];
					}

					if (syncableEntity != null)
					{
						// Do not allow sync ID to change for entities.
						if (Options.MaintainSyncId && entry.CurrentValues.Properties.Any(x => x.Name == nameof(ISyncEntity.SyncId)))
						{
							syncableEntity.SyncId = (Guid) entry.OriginalValues["SyncId"];
						}
					}

					if (modifiableEntity != null && maintainModifiedOnDate)
					{
						// Update modified to now for new entities.
						modifiableEntity.ModifiedOn = now;
					}
					
					entity.EntityModified();
					break;

				case EntityState.Deleted:
					if (syncableEntity != null && !Options.PermanentSyncEntityDeletions)
					{
						syncableEntity.IsDeleted = true;
						syncableEntity.ModifiedOn = now;
						entry.State = EntityState.Modified;
					}
					else
					{
						_collectionChangeTracker.AddRemovedEntity(entity);
					}

					entity.EntityDeleted();
					break;
			}
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion
	}
}