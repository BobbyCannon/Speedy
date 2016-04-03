#region References

using System;
using System.Data.Entity;
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
	public abstract class EntityFrameworkDatabase : DbContext, IDatabase
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database.
		/// </summary>
		/// <param name="nameOrConnectionString"> The name of the connection string or the actually connection string. </param>
		/// <param name="options"> The options for this database. </param>
		protected EntityFrameworkDatabase(string nameOrConnectionString, DatabaseOptions options)
			: base(nameOrConnectionString)
		{
			Options = options ?? DatabaseOptions.GetDefaults();
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
		/// Saves all changes made in this context to the underlying database.
		/// </summary>
		/// <returns>
		/// The number of objects written to the underlying database.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException"> Thrown if the context has been disposed. </exception>
		public override int SaveChanges()
		{
			try
			{
				foreach (var entry in ChangeTracker.Entries())
				{
					ProcessEntity(entry);
				}

				return base.SaveChanges();
			}
			catch (Exception ex)
			{
				ProcessException(ex);
				throw;
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
				.Where(type => type.BaseType != null && (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof (EntityTypeConfiguration<>)));

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

			var entity = entry.Entity as Entity;
			if (entity == null || !Options.MaintainDates)
			{
				return;
			}

			var modifiableEntity = entity as ModifiableEntity;

			// Check to see if the entity was added.
			if (entry.State == EntityState.Added)
			{
				// Make sure the modified on value matches created on for new items.
				entity.CreatedOn = DateTime.UtcNow;
				if (modifiableEntity != null)
				{
					modifiableEntity.ModifiedOn = entity.CreatedOn;
				}
			}

			// Check to see if the entity was modified.
			if (entry.State == EntityState.Modified)
			{
				if (entry.CurrentValues.PropertyNames.Contains("CreatedOn"))
				{
					// Do not allow created on to change for entities.
					entity.CreatedOn = (DateTime) entry.OriginalValues["CreatedOn"];
				}

				if (modifiableEntity != null)
				{
					// Update modified to now for new entities.
					modifiableEntity.ModifiedOn = DateTime.UtcNow;
				}
			}
		}

		#endregion
	}
}