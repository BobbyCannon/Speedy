#region References

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;

#endregion

namespace Speedy.EntityFramework
{
    /// <summary>
    /// An implementation of <see cref="IDatabaseInitializer{TContext}" /> that will use Code First Migrations
    /// to update the database to the latest version using the provided context. This allows you to migration multiple
    /// databases using the same database initializer.
    /// </summary>
    public class MigrateDatabaseToLatestVersionByContext<TContext, TMigrationConfiguration> : IDatabaseInitializer<TContext>
        where TContext : DbContext
        where TMigrationConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        #region Methods

        /// <summary>
        /// Executes the strategy to initialize the database for the given context.
        /// </summary>
        /// <param name="context">The context that should be migrated using in database connection string.</param>
        public void InitializeDatabase(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var connectionString = context.Database.Connection.ConnectionString;
            var configuration = new TMigrationConfiguration();
            configuration.TargetDatabase = new DbConnectionInfo(connectionString, "System.Data.SqlClient");
            configuration.CommandTimeout = int.MaxValue;

            var migrator = new DbMigrator(configuration);
            migrator.Configuration.CommandTimeout = int.MaxValue;
            migrator.Update();
        }

        #endregion
    }
}