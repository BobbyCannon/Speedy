#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.Configuration;
using Speedy.Storage;

#endregion

namespace Speedy
{
	[Serializable]
	public abstract class Database : IDatabase
	{
		#region Constructors

		protected Database(string filePath)
		{
			FilePath = filePath;
			Mappings = new List<IPropertyConfiguration>();
			Repositories = new Dictionary<string, IEntityRepository>();
			Relationships = new Dictionary<string, object[]>();
		}

		#endregion

		#region Properties

		private string FilePath { get; }

		private ICollection<IPropertyConfiguration> Mappings { get; }

		private Dictionary<string, object[]> Relationships { get; }

		private Dictionary<string, IEntityRepository> Repositories { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Repositories.Values.ForEach(x => x.Reset());
		}

		public IEntityRepository<T> GetReadOnlyRepository<T>() where T : Entity, new()
		{
			return GetEntityRepository<T>();
		}

		public IEntityRepository<T> GetRepository<T>() where T : Entity, new()
		{
			return GetEntityRepository<T>();
		}

		public virtual int SaveChanges()
		{
			var response = 0;
			Repositories.Values.ForEach(x => x.ValidateEntities());
			Repositories.Values.ForEach(x => x.UpdateRelationships());
			Repositories.Values.ForEach(x => x.AssignKeys());
			Repositories.Values.ForEach(x => x.UpdateRelationships());
			Repositories.Values.ForEach(x => response += x.SaveChanges());
			return response;
		}

		protected void HasMany<T1, T2>(Expression<Func<T1, T2>> entity, Expression<Func<T1, int>> foreignKey, string key = null)
			where T1 : Entity
			where T2 : Entity
		{
			var theKey = key ?? typeof (T2).Name + typeof (T1).Name;
			var repository = Repositories.FirstOrDefault(x => x.Key == typeof (T1).FullName).Value;
			Relationships.Add(theKey, new object[] { repository, entity, foreignKey });
		}

		protected PropertyConfiguration<T> Property<T>(Expression<Func<T, object>> expression) where T : Entity
		{
			var response = new PropertyConfiguration<T>(expression);

			Mappings.Add(response);

			return response;
		}

		private static void AssignNewValue<T1, T2>(T1 obj, Expression<Func<T1, T2>> expression, T2 value)
		{
			var valueParameterExpression = Expression.Parameter(typeof (T2));
			var targetExpression = (expression.Body as UnaryExpression)?.Operand ?? expression.Body;

			var assign = Expression.Lambda<Action<T1, T2>>
				(
					Expression.Assign(targetExpression, Expression.Convert(valueParameterExpression, targetExpression.Type)),
					expression.Parameters.Single(), valueParameterExpression
				);

			assign.Compile().Invoke(obj, value);
		}

		private IEnumerable<Entity> BuildRelationship(Type entityType, Type collectionType, Entity entity, IEnumerable collection, string key)
		{
			var genericMethod = GetGenericMethod("BuildRelationship", new[] { entityType, collectionType }, entityType, typeof (IEnumerable), typeof (string));
			return (IEnumerable<Entity>) genericMethod.Invoke(this, new object[] { entity, collection, key });
		}

		/// <summary>
		/// Builds relationship repository for the entity provided.
		/// </summary>
		/// <typeparam name="T1"> The type of the entity with the relationship. </typeparam>
		/// <typeparam name="T2"> The type of the related collection. </typeparam>
		/// <param name="entity"> The entity to process. </param>
		/// <param name="collection"> The entities to add or update to the repository. </param>
		/// <param name="key"> The key of the relationship </param>
		/// <returns> The repository for the relationship. </returns>
		// ReSharper disable once UnusedMember.Local
		private RelationshipRepository<T2> BuildRelationship<T1, T2>(T1 entity, IEnumerable collection, string key)
			where T1 : Entity, new()
			where T2 : Entity, new()
		{
			if (!Relationships.ContainsKey(key))
			{
				return null;
			}

			var value = Relationships[key];
			var repository = (IEntityRepository<T2>) value[0];
			var foreignEntity = (Expression<Func<T2, T1>>) value[1];
			var foreignKey = (Expression<Func<T2, int>>) value[2];
			var entityFunction = foreignEntity.Compile();
			var keyFunction = foreignKey.Compile();

			var response = new RelationshipRepository<T2>((EntityRepository<T2>) repository, x =>
			{
				var invokedKey = keyFunction.Invoke(x);
				if (invokedKey == entity.Id)
				{
					return true;
				}

				var invokedEntity = entityFunction.Invoke(x);
				return invokedEntity == entity;
			}, x =>
			{
				if (entity?.Id > 0)
				{
					AssignNewValue(x, foreignKey, entity.Id);
				}
				else
				{
					AssignNewValue(x, foreignEntity, entity);
				}
			}, x =>
			{
				var invokedEntity = entityFunction.Invoke(x);
				if (invokedEntity == null)
				{
					return;
				}

				var invokedKey = keyFunction.Invoke(x);
				if (invokedKey != invokedEntity.Id)
				{
					invokedEntity.Id = invokedKey;
				}
			});

			collection.ForEach(x => response.AddOrUpdate((T2) x));
			return response;
		}

		private EntityRepository<T> GetEntityRepository<T>() where T : Entity, new()
		{
			var type = typeof (T);
			var key = type.FullName;

			if (Repositories.ContainsKey(key))
			{
				return (EntityRepository<T>) Repositories[key];
			}

			var repository = new EntityRepository<T>(FilePath);
			repository.UpdateEntityRelationships += UpdateEntityRelationships;
			repository.ValidateEntity += ValidateEntity;

			Repositories.Add(key, repository);
			return repository;
		}

		private static MethodInfo GetGenericMethod(string methodName, Type[] typeArgs, params Type[] argTypes)
		{
			var methods1 = typeof (Database).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
			var methods = methods1.Where(m => m.Name == methodName
				&& typeArgs.Length == m.GetGenericArguments().Length
				&& argTypes.Length == m.GetParameters().Length);

			foreach (var method in methods)
			{
				var m = method.MakeGenericMethod(typeArgs);
				if (m.GetParameters().Select((p, i) => p.ParameterType == argTypes[i]).All(x => x))
				{
					return m;
				}
			}

			return null;
		}

		private void UpdateEntityChildRelationships(Entity item, Entity entity)
		{
			var entityType = entity.GetType();
			var itemType = item.GetType();
			var properties = itemType.GetProperties().ToList();

			var entityRelationship = properties.FirstOrDefault(x => x.Name == entityType.Name);
			entityRelationship?.SetValue(item, entity, null);

			var entityRelationshipId = properties.FirstOrDefault(x => x.Name == entityType.Name + "Id");
			entityRelationshipId?.SetValue(item, entity.Id, null);
		}

		private void UpdateEntityCollectionRelationships(Entity entity, IEnumerable<PropertyInfo> properties, Type entityType)
		{
			var enumerableType = typeof (IEnumerable);
			var collectionRelationships = properties
				.Where(x => x.GetAccessors()[0].IsVirtual)
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				// Check to see if we have a repository for the generic type.
				var collectionType = relationship.PropertyType.GetGenericArguments()[0];
				if (!Repositories.ContainsKey(collectionType.FullName))
				{
					continue;
				}

				// Converts the relationship to a relationship (filtered) repository.
				var currentCollection = (IEnumerable<Entity>) relationship.GetValue(entity, null);
				var currentCollectionType = currentCollection.GetType();

				if (currentCollectionType.Name == typeof (RelationshipRepository<>).Name)
				{
					// We are already a relationship repository so just update the relationships
					continue;
				}

				// Add any existing entities to the new filtered collection.
				foreach (var item in currentCollection)
				{
					//relationshipFilter.AddOrUpdate(item);
					UpdateEntityChildRelationships(item, entity);
					UpdateEntityDirectRelationships(item);
				}

				// See if the entity has a relationship filter.
				var key1 = entityType.Name + collectionType.Name;
				var key2 = entityType.Name + relationship.Name;

				var relationshipFilter = BuildRelationship(entityType, collectionType, entity, currentCollection, key1)
					?? BuildRelationship(entityType, collectionType, entity, currentCollection, key2);

				// Check to see if the custom memory context has a filter method.
				if (relationshipFilter == null)
				{
					// No filter so there's nothing to do.
					continue;
				}

				// Update relationship collection to the new filtered collection.
				relationship.SetValue(entity, relationshipFilter, null);
			}
		}

		private void UpdateEntityDirectRelationships(Entity entity)
		{
			var entityType = entity.GetType();
			var properties = entityType.GetProperties().ToList();
			UpdateEntityDirectRelationships(entity, properties, entityType);
		}

		private void UpdateEntityDirectRelationships(Entity entity, ICollection<PropertyInfo> properties, Type entityType)
		{
			var baseEntityType = typeof (Entity);
			var entityRelationships = properties
				.Where(x => x.GetAccessors()[0].IsVirtual)
				.Where(x => baseEntityType.IsAssignableFrom(x.PropertyType))
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var otherEntity = entityRelationship.GetValue(entity, null) as Entity;
				var entityRelationshipIdProperty = properties.FirstOrDefault(x => x.Name == entityRelationship.Name + "Id");

				if (otherEntity == null && entityRelationshipIdProperty != null)
				{
					var otherEntityId = (int?) entityRelationshipIdProperty.GetValue(entity, null);
					if (otherEntityId.HasValue && otherEntityId != 0 && Repositories.ContainsKey(entityRelationship.PropertyType.FullName))
					{
						var repository = Repositories[entityRelationship.PropertyType.FullName];
						otherEntity = repository.GetEntity(otherEntityId);
						entityRelationship.SetValue(entity, otherEntity, null);
					}
				}
				else if (otherEntity != null && entityRelationshipIdProperty != null)
				{
					// Check to see if this is a new child entity.
					if (otherEntity.Id == 0)
					{
						var repository = Repositories[entityRelationship.PropertyType.FullName];
						var repositoryType = repository.GetType();

						if (otherEntity.GetType() == entityType)
						{
							repositoryType.GetMethod("InsertBefore").Invoke(repository, new object[] { otherEntity, entity });
						}
						else
						{
							repositoryType.GetMethod("Add").Invoke(repository, new object[] { otherEntity });
						}
					}

					var otherEntityId = (int?) entityRelationshipIdProperty.GetValue(entity, null);
					if (otherEntityId != otherEntity.Id)
					{
						// resets entityId to entity.Id if it does not match
						entityRelationshipIdProperty.SetValue(entity, otherEntity.Id, null);
					}
				}
			}
		}

		private void UpdateEntityRelationships(Entity entity)
		{
			var entityType = entity.GetType();
			var properties = entityType.GetProperties().ToList();

			UpdateEntityDirectRelationships(entity, properties, entityType);
			UpdateEntityCollectionRelationships(entity, properties, entityType);
		}

		private void ValidateEntity(Entity entity)
		{
			foreach (var validation in Mappings.Where(x => x.IsMappingFor(entity)))
			{
				validation.Validate(entity);
			}
		}

		#endregion
	}
}