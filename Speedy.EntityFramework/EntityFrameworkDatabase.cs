#region References

using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents an Entity Framework Speedy database.
	/// </summary>
	public abstract class EntityFrameworkDatabase : DbContext
	{
		#region Fields

		private int _saveChangeCount;

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

		/// <summary>
		/// Gets a repository that is read only. This does not allow for any alterations (add, updates, removes, etc).
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		public IRepository<T, T2> GetReadOnlyRepository<T, T2>() where T : Entity<T2>, new()
		{
			return new ReadOnlyEntityFrameworkRepository<T, T2>(Set<T>());
		}

		/// <summary>
		/// Gets a repository of the requested entity.
		/// </summary>
		/// <typeparam name="T"> The type of the entity to get a repository for. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository of entities requested. </returns>
		public IRepository<T, T2> GetRepository<T, T2>() where T : Entity<T2>, new()
		{
			return new EntityFrameworkRepository<T, T2>(Set<T>());
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

			var entity = entry.Entity as IEntity;
			if (entity == null)
			{
				return;
			}

			var createdEntity = entity as ICreatedEntity;
			var modifiableEntity = entity as IModifiableEntity;
			var maintainedEntity = Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
			var maintainDates = maintainedEntity && Options.MaintainDates;
			var now = DateTime.UtcNow;
			
			// Check to see if the entity was added.
			switch (entry.State)
			{
				case EntityState.Added:
					entity.EntityAdded();
					
					if (createdEntity != null && maintainDates)
					{
						// Make sure the modified on value matches created on for new items.
						createdEntity.CreatedOn = now;
					}
					
					if (modifiableEntity != null && maintainDates)
					{
						modifiableEntity.ModifiedOn = now;
					}
					break;

				case EntityState.Modified:
					entity.EntityModified();

					if (createdEntity != null && maintainDates && entry.CurrentValues.PropertyNames.Contains("CreatedOn"))
					{
						// Do not allow created on to change for entities.
						createdEntity.CreatedOn = (DateTime) entry.OriginalValues["CreatedOn"];
					}

					if (modifiableEntity != null && maintainDates)
					{
						// Update modified to now for new entities.
						modifiableEntity.ModifiedOn = now;
					}
					break;

				case EntityState.Deleted:
					entity.EntityDeleted();
					break;
			}
		}

		#endregion
	}
}