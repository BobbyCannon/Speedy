#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Speedy.EntityFrameworkCore.Internal;
using Speedy.Sync;

#endregion

namespace Speedy.EntityFrameworkCore
{
    /// <summary>
    /// Represents an Entity Framework Speedy database.
    /// </summary>
    public abstract class EntityFrameworkDatabase : DbContext, ISyncableDatabase
    {
        #region Fields

        private readonly string _nameOrConnectionString;
        private int _saveChangeCount;
        private readonly ConcurrentDictionary<string, ISyncableRepository> _syncableRepositories;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an instance of the database.
        /// </summary>
        /// <param name="nameOrConnectionString"> The name of the connection string or the actually connection string. </param>
        /// <param name="options"> The options for this database. </param>
        protected EntityFrameworkDatabase(string nameOrConnectionString, DatabaseOptions options)
        {
            _nameOrConnectionString = nameOrConnectionString;
            Options = options ?? new DatabaseOptions();
            SyncTombstones = GetRepository<SyncTombstone>();

            _syncableRepositories = new ConcurrentDictionary<string, ISyncableRepository>();

            if (Options.DetectSyncableRepositories)
            {
                DetectSyncableRepositories();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the options for this database.
        /// </summary>
        public DatabaseOptions Options { get; }

        /// <summary>
        /// Gets the sync tombstone repository.
        /// </summary>
        public IRepository<SyncTombstone> SyncTombstones { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Discard all changes made in this context to the underlying database.
        /// </summary>
        public int DiscardChanges()
        {
            var count = 0;

            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        foreach (var metaProperty in entry.Metadata.GetProperties())
                        {
                            var property = entry.Property(metaProperty.Name);
                            if (property.IsModified)
                            {
                                property.CurrentValue = property.OriginalValue;
                            }
                        }
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

        /// <summary>
        /// Gets a repository that is read only. This does not allow for any alterations (add, updates, removes, etc).
        /// </summary>
        /// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
        /// <returns> The repository of entities requested. </returns>
        public IRepository<T> GetReadOnlyRepository<T>() where T : Entity, new()
        {
            return new ReadOnlyEntityFrameworkRepository<T>(Set<T>());
        }

        /// <summary>
        /// Gets a repository of the requested entity.
        /// </summary>
        /// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
        /// <returns> The repository of entities requested. </returns>
        public IRepository<T> GetRepository<T>() where T : Entity, new()
        {
            return new EntityFrameworkRepository<T>(Set<T>());
        }

        /// <summary>
        /// Gets a list of syncable repositories. The repositories will be ordered base on DatabaseOptions.SyncOrder.
        /// </summary>
        /// <returns> The list of syncable repositories. </returns>
        public IEnumerable<ISyncableRepository> GetSyncableRepositories()
        {
            if (Options.SyncOrder.Length <= 0)
            {
                return _syncableRepositories.Values;
            }

            var order = Options.SyncOrder.Reverse().ToList();
            var ordered = _syncableRepositories.OrderBy(x => x.Key == order[0]);
            ordered = order.Skip(1).Aggregate(ordered, (current, key) => current.ThenBy(x => x.Key == key));

            return ordered.Select(x => x.Value);
        }

        /// <summary>
        /// Gets a syncable repository of the requested entity.
        /// </summary>
        /// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
        /// <returns> The repository of entities requested. </returns>
        public ISyncableRepository<T> GetSyncableRepository<T>() where T : SyncEntity, new()
        {
            return new EntityFrameworkSyncableRepository<T>(Set<T>());
        }

        /// <summary>
        /// Gets a syncable repository of the requested entity.
        /// </summary>
        /// <returns> The repository of entities requested. </returns>
        public ISyncableRepository GetSyncableRepository(Type type)
        {
            ISyncableRepository repository;
            if (_syncableRepositories.TryGetValue(type.FullName, out repository))
            {
                return repository;
            }

            var methods = GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
            var setMethod = methods.First(x => (x.Name == "Set") && x.IsGenericMethodDefinition);
            var method = setMethod.MakeGenericMethod(type);
            var entitySet = method.Invoke(this, null);
            var repositoryType = typeof(EntityFrameworkSyncableRepository<>).MakeGenericType(type);
            repository = Activator.CreateInstance(repositoryType, entitySet) as ISyncableRepository;

            _syncableRepositories.AddOrUpdate(type.FullName, repository, (k, v) => repository);

            return repository;
        }

        /// <summary>
        /// Gets a list of sync tombstones that represent deleted entities.
        /// </summary>
        /// <param name="filter"> The filter to use. </param>
        /// <returns> The list of sync tombstones. </returns>
        public IQueryable<SyncTombstone> GetSyncTombstones(Expression<Func<SyncTombstone, bool>> filter)
        {
            return SyncTombstones.Where(filter);
        }

        /// <summary>
        /// Gets a list of sync tombstones that represent deleted entities.
        /// </summary>
        /// <param name="since"> The date and time get changes for. </param>
        /// <returns> The list of sync tombstones. </returns>
        public IEnumerable<SyncObject> GetSyncTombstones(DateTime since)
        {
            return SyncTombstones.Where(x => x.CreatedOn >= since)
                .ToList()
                .Select(x => x.ToSyncObject())
                .Where(x => x != null)
                .ToList();
        }

        /// <summary>
        /// Removes sync tombstones that represent match the filter.
        /// </summary>
        /// <param name="filter"> The filter to use. </param>
        public void RemoveSyncTombstones(Expression<Func<SyncTombstone, bool>> filter)
        {
            SyncTombstones.Remove(filter);
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
                ChangeTracker.Entries().ForEach(ProcessEntity);

                // The local relationships may have changed. We need keep our sync IDs in sync with 
                // any relationships that may have changed.
                ChangeTracker.Entries().ForEach(x => (x.Entity as SyncEntity)?.UpdateLocalSyncIds());

                var response = base.SaveChanges();

                if (ChangeTracker.Entries().Any(x => (x.State != EntityState.Detached) && (x.State != EntityState.Unchanged)))
                {
                    response += SaveChanges();
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_nameOrConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting
        /// model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via
        /// <see
        ///     cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />
        /// )
        /// then this method will not be run.
        /// </remarks>
        /// <param name="modelBuilder">
        /// The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var assembly = Assembly.GetAssembly(GetType());
            var typesToRegister = assembly.GetTypes()
                .Where(type => (type.BaseType != null) && type.BaseType.IsGenericType && (type.BaseType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration)));

            foreach (var type in typesToRegister)
            {
                var instance = (IEntityTypeConfiguration) Activator.CreateInstance(type);
                instance.Configure(modelBuilder);
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Allows data context a chance to process the exception that occurred during save changes.
        /// </summary>
        /// <param name="exception"> The exception that occurred when trying to save changes. </param>
        protected virtual void ProcessException(Exception exception)
        {
        }

        /// <summary>
        /// Reads all repositories and puts all the syncable ones in an internal list.
        /// </summary>
        private void DetectSyncableRepositories()
        {
            var type = GetType();
            var test = type.GetCachedProperties();
            var properties = test.Where(x => x.PropertyType.Name == typeof(IRepository<>).Name).ToList();

            _syncableRepositories.Clear();
            var syncEntityType = typeof(SyncEntity);
            var syncTombstoneType = typeof(SyncTombstone);

            foreach (var property in properties)
            {
                var genericType = property.PropertyType.GetGenericArguments().First();

                if (syncTombstoneType.IsAssignableFrom(genericType) || !syncEntityType.IsAssignableFrom(genericType))
                {
                    continue;
                }

                GetSyncableRepository(genericType);
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

            var entity = entry.Entity as Entity;
            if (entity == null)
            {
                return;
            }

            var modifiableEntity = entity as ModifiableEntity;
            var syncableEntity = entity as SyncEntity;
            var maintainedEntity = Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
            var maintainDates = maintainedEntity && Options.MaintainDates;
            var maintainSyncId = maintainedEntity && Options.MaintainSyncId;

            // Check to see if the entity was added.
            switch (entry.State)
            {
                case EntityState.Added:
                    if (maintainDates)
                    {
                        // Make sure the modified on value matches created on for new items.
                        entity.CreatedOn = DateTime.UtcNow;
                    }

                    if (syncableEntity != null)
                    {
                        if (maintainSyncId && (syncableEntity.SyncId == Guid.Empty))
                        {
                            syncableEntity.SyncId = Guid.NewGuid();
                        }
                    }

                    if ((modifiableEntity != null) && maintainDates)
                    {
                        modifiableEntity.ModifiedOn = entity.CreatedOn;
                    }
                    break;

                case EntityState.Modified:
                    if (maintainDates && entry.Property("CreatedOn").IsModified)
                    {
                        // Do not allow created on to change for entities.
                        entity.CreatedOn = (DateTime)entry.Property("CreatedOn").OriginalValue;
                    }

                    if (syncableEntity != null)
                    {
                        // Do not allow sync ID to change for entities.
                        if (Options.MaintainSyncId && entry.Property("SyncId").IsModified)
                        {
                            syncableEntity.SyncId = (Guid) entry.Property("SyncId").OriginalValue;
                        }
                    }

                    if ((modifiableEntity != null) && maintainDates)
                    {
                        // Update modified to now for new entities.
                        modifiableEntity.ModifiedOn = DateTime.UtcNow;
                    }
                    break;

                case EntityState.Deleted:
                    if (syncableEntity != null)
                    {
                        if (syncableEntity.SyncId == Guid.Empty)
                        {
                            throw new InvalidOperationException("Cannot tombstone this entity because the sync ID has not been set.");
                        }

                        SyncTombstones?.Add(syncableEntity.ToSyncTombstone(Options.SyncTombstoneReferenceId));
                    }
                    break;
            }
        }

        #endregion
    }

    public interface IEntityTypeConfiguration
    {
        #region Methods

        void Configure(ModelBuilder instance);

        #endregion
    }
}