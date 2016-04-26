#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.EntityFramework.Internal;
using Speedy.Sync;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents an Entity Framework Speedy database.
	/// </summary>
	public abstract class EntityFrameworkDatabase : DbContext, ISyncableDatabase
	{
		#region Fields

		private int _saveChangeCount;
		private readonly ConcurrentDictionary<string, ISyncableRepository> _syncableRepositories;
		private IRepository<SyncTombstone> _syncTombstones;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database.
		/// </summary>
		/// <param name="nameOrConnectionString"> The name of the connection string or the actually connection string. </param>
		/// <param name="options"> The options for this database. </param>
		protected EntityFrameworkDatabase(string nameOrConnectionString, DatabaseOptions options)
			: base(nameOrConnectionString)
		{
			Options = options ?? new DatabaseOptions();

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

		#endregion

		#region Methods

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
			var setMethod = methods.First(x => x.Name == "Set" && x.IsGenericMethodDefinition);
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
			return _syncTombstones.Where(filter);
		}

		/// <summary>
		/// Gets a list of sync tombstones that represent deleted entities.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of sync tombstones. </returns>
		public IEnumerable<SyncObject> GetSyncTombstones(DateTime since)
		{
			return _syncTombstones
				.Where(x => x.CreatedOn >= since)
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
			_syncTombstones.Remove(filter);
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

				if (ChangeTracker.Entries().Any(x => x.State != EntityState.Detached && x.State != EntityState.Unchanged))
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
		/// ModelBuidler, but note that this can seriously degrade performance. More control over caching is provided through use
		/// of the DbModelBuilder and
		/// DbContextFactory classes directly.
		/// </remarks>
		/// <param name="modelBuilder"> The builder that defines the model for the context being created. </param>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var assembly = Assembly.GetAssembly(GetType());
			var typesToRegister = assembly.GetTypes()
				.Where(type => type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

			foreach (var type in typesToRegister)
			{
				dynamic instance = Activator.CreateInstance(type);
				modelBuilder.Configurations.Add(instance);
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
			var properties = test
				.Where(x => x.PropertyType.Name == typeof(IRepository<>).Name)
				.ToList();

			_syncableRepositories.Clear();
			var syncEntityType = typeof(SyncEntity);
			var syncTombstoneType = typeof(SyncTombstone);

			foreach (var property in properties)
			{
				var genericType = property.PropertyType.GetGenericArguments().First();

				if (syncTombstoneType.IsAssignableFrom(genericType))
				{
					_syncTombstones = (IRepository<SyncTombstone>) property.GetValue(this, null);
					continue;
				}

				if (!syncEntityType.IsAssignableFrom(genericType))
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
		private void ProcessEntity(DbEntityEntry entry)
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

			// Check to see if the entity was added.
			switch (entry.State)
			{
				case EntityState.Added:
					if (Options.MaintainDates)
					{
						// Make sure the modified on value matches created on for new items.
						entity.CreatedOn = DateTime.UtcNow;
					}

					if (syncableEntity != null)
					{
						if (Options.MaintainSyncId && syncableEntity.SyncId == Guid.Empty)
						{
							syncableEntity.SyncId = Guid.NewGuid();
						}
					}

					if (modifiableEntity != null && Options.MaintainDates)
					{
						modifiableEntity.ModifiedOn = entity.CreatedOn;
					}
					break;

				case EntityState.Modified:
					if (Options.MaintainDates && entry.CurrentValues.PropertyNames.Contains("CreatedOn"))
					{
						// Do not allow created on to change for entities.
						entity.CreatedOn = (DateTime) entry.OriginalValues["CreatedOn"];
					}

					if (syncableEntity != null)
					{
						// Do not allow sync ID to change for entities.
						if (Options.MaintainSyncId && entry.CurrentValues.PropertyNames.Contains("SyncId"))
						{
							syncableEntity.SyncId = (Guid) entry.OriginalValues["SyncId"];
						}
					}

					if (modifiableEntity != null && Options.MaintainDates)
					{
						// Update modified to now for new entities.
						modifiableEntity.ModifiedOn = DateTime.UtcNow;
					}
					break;

				case EntityState.Deleted:
					if (syncableEntity != null)
					{
						_syncTombstones?.Add(syncableEntity.ToSyncTombstone());
					}
					break;
			}
		}

		#endregion
	}
}