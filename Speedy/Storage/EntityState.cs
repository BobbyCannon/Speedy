#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Speedy.Extensions;

#endregion

namespace Speedy.Storage
{
	internal class EntityState
	{
		#region Methods

		/// <summary>
		/// Represents properties we want to "track" changes for.
		/// </summary>
		/// <param name="type"> The type of the entity. </param>
		/// <returns> The properties for state tracking. </returns>
		internal static IEnumerable<PropertyInfo> GetStateProperties(Type type)
		{
			return type
				.GetCachedProperties()
				.Where(x =>
				{
					if (x.Name == "Id")
					{
						return true;
					}

					if (x.IsVirtual())
					{
						return false;
					}

					var accessors = x.GetAccessors();
					if (accessors.Length < 2)
					{
						return false;
					}

					return true;
				})
				.ToList();
		}

		#endregion
	}

	internal class EntityState<T, T2> : EntityState
		where T : Entity<T2>
	{
		#region Fields

		private readonly Repository<T, T2> _repository;
		private readonly Type _type;

		#endregion

		#region Constructors

		public EntityState(Repository<T, T2> repository, T entity, T oldEntity, EntityStateType state)
		{
			_repository = repository;
			_type = typeof(T);

			Entity = entity;
			Entity.PropertyChanged += EntityOnPropertyChanged;
			OldEntity = oldEntity;
			State = state;
		}

		#endregion

		#region Properties

		public T Entity { get; }

		public T OldEntity { get; }

		public EntityStateType State { get; set; }

		#endregion

		#region Methods

		public IEnumerable<PropertyInfo> GetChangedProperties()
		{
			var properties = _type.GetCachedProperties();
			var changedProperties = Entity.ChangedProperties;
			return properties.Where(x => changedProperties.Contains(x.Name)).ToList();
		}

		public void Reset()
		{
			UpdateEntity(Entity, OldEntity);
			ResetChangeTracking();
		}

		public void ResetChangeTracking()
		{
			Entity.ResetHasChanges();
			OldEntity.ResetHasChanges();
			State = EntityStateType.Unmodified;
		}

		public void ResetEvents()
		{
			Entity.PropertyChanged -= EntityOnPropertyChanged;
		}

		public void SaveChanges()
		{
			UpdateEntity(OldEntity, Entity);
			ResetChangeTracking();
		}

		internal void RefreshState()
		{
			if (!CompareEntity(Entity, OldEntity))
			{
				State = EntityStateType.Modified;
			}
		}

		/// <summary>
		/// Update the entity with the new values.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="updatedEntity"> The new values to update the entity with. </param>
		internal void UpdateEntity(Entity<T2> entity, Entity<T2> updatedEntity)
		{
			var properties = GetStateProperties(_type);

			foreach (var property in properties)
			{
				var currentValue = property.GetValue(entity, null);
				var updatedValue = property.GetValue(updatedEntity, null);

				if (currentValue != updatedValue)
				{
					property.SetValue(entity, updatedValue, null);
				}
			}
		}

		private bool CompareEntity(T entity1, T entity2)
		{
			if ((entity1 == null) && (entity2 == null))
			{
				return true;
			}

			if ((entity1 == null) || (entity2 == null))
			{
				return false;
			}

			var properties = GetStateProperties(_type);
			foreach (var property in properties)
			{
				var value1 = property.GetValue(entity1, null);
				var value2 = property.GetValue(entity2, null);
				if (!Equals(value1, value2))
				{
					return false;
				}
			}

			return true;
		}

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if ((State == EntityStateType.Unmodified) && Entity.HasChanges())
			{
				State = EntityStateType.Modified;
			}
		}

		#endregion
	}
}