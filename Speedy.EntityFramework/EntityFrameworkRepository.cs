#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Speedy.EntityFramework.Sql;
using Speedy.Exceptions;
using Speedy.Extensions;

#endregion

namespace Speedy.EntityFramework
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The entity type this collection is for. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public class EntityFrameworkRepository<T, T2> : IRepository<T, T2> where T : Entity<T2>
	{
		#region Fields

		/// <summary>
		/// The set of the entities.
		/// </summary>
		protected readonly DbSet<T> Set;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a repository.
		/// </summary>
		/// <param name="database"> The database where this repository resides. </param>
		/// <param name="set"> The database set this repository is for. </param>
		public EntityFrameworkRepository(EntityFrameworkDatabase database, DbSet<T> set)
		{
			Database = database;
			Set = set;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The database where this repository resides.
		/// </summary>
		public EntityFrameworkDatabase Database { get; }

		/// <summary>
		/// Gets the type of the element(s) that are returned when the expression tree associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" /> is executed.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree
		/// associated with this object is executed.
		/// </returns>
		public Type ElementType => ((IQueryable<T>) Set).ElementType;

		/// <summary>
		/// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of
		/// <see cref="T:System.Linq.IQueryable" />.
		/// </returns>
		public Expression Expression => ((IQueryable<T>) Set).Expression;

		/// <summary>
		/// Gets the query provider that is associated with this data source.
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.
		/// </returns>
		public IQueryProvider Provider => ((IQueryable<T>) Set).Provider;

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(T entity)
		{
			Set.Add(entity);
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(T entity)
		{
			if (Set.Any(x => x.Id.Equals(entity.Id)))
			{
				Set.Update(entity);
				return;
			}

			Set.Add(entity);
		}

		/// <inheritdoc />
		public int BulkAdd(params T[] entities)
		{
			entities.ForEach(UpdateRelationships);

			switch (Database.GetProviderType())
			{
				case DatabaseProviderType.Sqlite:
				{
					Database.Database.OpenConnection();
					var connection = (SqliteConnection) Database.Database.GetDbConnection();

					try
					{
						var transaction = (SqliteTransaction) (Database.Database.CurrentTransaction?.GetDbTransaction()
							?? connection.BeginTransaction(IsolationLevel.ReadCommitted));
						var statement = SqlBuilder.GetSqlInsert<T>(Database);
						var command = new SqliteCommand(statement.Query.ToString(), connection, transaction);

						foreach (var entity in entities)
						{
							SqlBuilder.UpdateCommand(statement, command, entity);
							command.ExecuteNonQuery();
						}

						transaction.Commit();

						return entities.Length;
					}
					finally
					{
						Database.Database.CloseConnection();
					}
				}
				case DatabaseProviderType.SqlServer:
				{
					Database.Database.OpenConnection();
					var connection = Database.Database.GetDbConnection();

					try
					{
						var transaction = Database.Database.CurrentTransaction;
						var options = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls;
						using var sqlBulkCopy = GetSqlBulkCopy((SqlConnection) connection, transaction, options);
						var dataTable = GetDataTable(entities, sqlBulkCopy);
						sqlBulkCopy.WriteToServer(dataTable);
						return dataTable.Rows.Count;
					}
					finally
					{
						Database.Database.CloseConnection();
					}
				}
				case DatabaseProviderType.Unknown:
				default:
					throw new NotSupportedException();
			}
		}

		/// <inheritdoc />
		public int BulkAdd(IEnumerable<T> entities)
		{
			return BulkAdd(entities.ToArray());
		}

		/// <inheritdoc />
		public int BulkAddOrUpdate(params T[] entities)
		{
			switch (Database.GetProviderType())
			{
				case DatabaseProviderType.Sqlite:
				{
					Database.Database.OpenConnection();
					var connection = (SqliteConnection) Database.Database.GetDbConnection();

					try
					{
						var transaction = (SqliteTransaction) (Database.Database.CurrentTransaction?.GetDbTransaction()
							?? connection.BeginTransaction(IsolationLevel.ReadCommitted));
						var statement = SqlBuilder.GetSqlInsertOrUpdate<T>(Database);
						var command = new SqliteCommand(statement.Query.ToString(), connection, transaction);

						foreach (var entity in entities)
						{
							UpdateRelationships(entity);
							SqlBuilder.UpdateCommand(statement, command, entity);
							command.ExecuteNonQuery();
						}

						transaction.Commit();

						return entities.Length;
					}
					finally
					{
						Database.Database.CloseConnection();
					}
				}
				case DatabaseProviderType.SqlServer:
				{
					Database.Database.OpenConnection();
					var connection = (SqlConnection) Database.Database.GetDbConnection();

					try
					{
						var transaction = (SqlTransaction) (Database.Database.CurrentTransaction?.GetDbTransaction()
							?? connection.BeginTransaction(IsolationLevel.ReadCommitted));
						var statement = SqlBuilder.GetSqlInsertOrUpdate<T>(Database);
						var command = new SqlCommand(statement.Query.ToString(), connection, transaction);

						foreach (var entity in entities)
						{
							UpdateRelationships(entity);
							SqlBuilder.UpdateCommand(statement, command, entity);
							command.ExecuteNonQuery();
						}

						transaction.Commit();

						return entities.Length;
					}
					finally
					{
						Database.Database.CloseConnection();
					}
				}
				case DatabaseProviderType.Unknown:
				default:
					throw new NotSupportedException();
			}
		}

		/// <inheritdoc />
		public int BulkAddOrUpdate(IEnumerable<T> entities)
		{
			return BulkAddOrUpdate(entities.ToArray());
		}

		/// <inheritdoc />
		public int BulkRemove(Expression<Func<T, bool>> filter)
		{
			var statement = SqlBuilder.GetSqlDelete(Database, this.Where(filter));
			return Database.Database.ExecuteSqlRaw(statement.Query.ToString(), statement.Parameters);
		}

		/// <inheritdoc />
		public int BulkUpdate(Expression<Func<T, bool>> filter, Expression<Func<T, T>> update)
		{
			var statement = SqlBuilder.GetSqlUpdate(Database, this.Where(filter), update);
			return Database.Database.ExecuteSqlRaw(statement.Query.ToString(), statement.Parameters);
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			return ((IQueryable<T>) Set).GetEnumerator();
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include)
		{
			return Including(include);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, object> Including(params Expression<Func<T, object>>[] includes)
		{
			return Including<object>(includes);
		}

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes)
		{
			var result = includes.Aggregate(Set.AsQueryable(), (current, include) => current.Include(include));
			if (result is Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T3> aiq)
			{
				return new EntityIncludableQueryable<T, T3>(aiq);
			}

			// Try to find the internal includable queryable, not good but it is what we have to do...
			var includableQueryType = (Type) typeof(EntityFrameworkQueryableExtensions)
				.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic)
				.FirstOrDefault(x => x.Name == "IncludableQueryable`2");

			// Check to ensure we found the type
			if (includableQueryType == null)
			{
				throw new SpeedyException("Critical: Need to look into IncludableQueryable");
			}

			// Create an instance of the includable queryable so we can pass it to ThenInclude
			var includableQueryTypeGeneric = includableQueryType.MakeGenericType(typeof(T), typeof(T3));
			var instance = Activator.CreateInstance(includableQueryTypeGeneric, result);
			return new EntityIncludableQueryable<T, T3>((Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<T, T3>) instance);
		}

		/// <inheritdoc />
		public void Remove(T2 id)
		{
			var entity = Set.Local.FirstOrDefault(x => Equals(x.Id, id));
			if (entity == null)
			{
				entity = Activator.CreateInstance<T>();
				entity.Id = id;
				Set.Attach(entity);
			}

			Set.Remove(entity);
		}

		/// <inheritdoc />
		public void Remove(T entity)
		{
			Set.Remove(entity);
		}

		/// <inheritdoc />
		public void Remove(Expression<Func<T, bool>> filter)
		{
			Set.RemoveRange(Set.Where(filter));
		}

		private DataTable GetDataTable(IEnumerable<T> entities, SqlBulkCopy sqlBulkCopy)
		{
			var dataTable = new DataTable();
			var columnValues = new Dictionary<string, object>();
			var type = typeof(T).GetRealType();
			var entityType = Database.Model.FindEntityType(type);
			var entityProperties = entityType.GetProperties().ToDictionary(a => a.Name, a => a);
			var properties = type.GetCachedProperties().Where(x => entityProperties.ContainsKey(x.Name)).ToList();
			var tableName = entityType.GetTableName();
			var schemaName = entityType.GetSchema();

			foreach (var property in properties)
			{
				var entityPropertyType = entityProperties[property.Name];
				var propertyType = property.PropertyType;
				var columnName = entityPropertyType.GetColumnName(StoreObjectIdentifier.Table(tableName, schemaName));
				var underlyingType = Nullable.GetUnderlyingType(propertyType);
				dataTable.Columns.Add(columnName, underlyingType ?? propertyType);
				columnValues.Add(property.Name, null);
			}

			foreach (var entity in entities)
			{
				foreach (var property in properties)
				{
					var propertyValue = property.GetMethod.Invoke(entity, null);
					columnValues[property.Name] = propertyValue;
				}

				var record = columnValues.Values.ToArray();
				dataTable.Rows.Add(record);
			}

			sqlBulkCopy.DestinationTableName = entityType.GetTableName();
			sqlBulkCopy.BatchSize = 2000;
			//sqlBulkCopy.BulkCopyTimeout = ?
			sqlBulkCopy.EnableStreaming = true;

			foreach (DataColumn item in dataTable.Columns)
			{
				sqlBulkCopy.ColumnMappings.Add(item.ColumnName, item.ColumnName);
			}

			return dataTable;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private SqlBulkCopy GetSqlBulkCopy(SqlConnection sqlConnection, IDbContextTransaction transaction, SqlBulkCopyOptions options)
		{
			if (transaction == null)
			{
				return new SqlBulkCopy(sqlConnection, options, null);
			}

			var sqlTransaction = (SqlTransaction) transaction.GetDbTransaction();
			return new SqlBulkCopy(sqlConnection, options, sqlTransaction);
		}

		private void UpdateRelationships(T entity)
		{
			var baseType = typeof(IEntity);
			var entityType = entity.GetRealType();
			var entityProperties = entityType.GetCachedProperties();
			var entityRelationships = entityType.GetCachedVirtualProperties().Where(x => baseType.IsAssignableFrom(x.PropertyType)).ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				if (!(entityRelationship.GetValue(entity, null) is IEntity otherEntity))
				{
					continue;
				}

				var otherEntityProperties = otherEntity.GetRealType().GetCachedProperties();
				var otherEntityIdProperty = otherEntityProperties.FirstOrDefault(x => x.Name == "Id");
				var entityRelationshipIdProperty = entityProperties.FirstOrDefault(x => x.Name == entityRelationship.Name + "Id");
				
				if (otherEntityIdProperty != null && entityRelationshipIdProperty != null)
				{
					var entityId = entityRelationshipIdProperty.GetValue(entity, null);
					var otherId = otherEntityIdProperty.GetValue(otherEntity);

					if (!Equals(entityId, otherId))
					{
						// resets entityId to entity.Id if it does not match
						entityRelationshipIdProperty.SetValue(entity, otherId, null);
					}
				}

				var otherEntitySyncIdProperty = otherEntityProperties.FirstOrDefault(x => x.Name == "SyncId");
				var entityRelationshipSyncIdProperty = entityProperties.FirstOrDefault(x => x.Name == entityRelationship.Name + "SyncId");
				
				if (otherEntitySyncIdProperty != null && entityRelationshipSyncIdProperty != null)
				{
					var entitySyncId = entityRelationshipSyncIdProperty.GetValue(entity, null);
					var otherSyncId = otherEntitySyncIdProperty?.GetValue(otherEntity);

					if (!Equals(entitySyncId, otherSyncId))
					{
						// resets entityId to entity.SyncId if it does not match
						entityRelationshipSyncIdProperty.SetValue(entity, otherSyncId, null);
					}
				}
			}
		}

		#endregion
	}
}