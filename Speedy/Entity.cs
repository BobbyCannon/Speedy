#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	/// <typeparam name="T"> The type of the entity key. </typeparam>
	public abstract class Entity<T> : IEntity
	{
		#region Fields

		/// <summary>
		/// Properties to ignore when updating.
		/// </summary>
		private HashSet<string> _excludedPropertiesForUpdate;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an entity
		/// </summary>
		protected Entity()
		{
			_excludedPropertiesForUpdate = new HashSet<string>(GetDefaultExclusionsForUpdate());
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the ID of the entity.
		/// </summary>
		public abstract T Id { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public virtual bool CanBeModified()
		{
			return true;
		}

		/// <inheritdoc />
		public virtual void EntityAdded()
		{
		}

		/// <inheritdoc />
		public virtual void EntityDeleted()
		{
		}

		/// <inheritdoc />
		public virtual void EntityModified()
		{
		}

		/// <inheritdoc />
		public void ExcludePropertiesForUpdate(params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				if (_excludedPropertiesForUpdate.Contains(propertyName))
				{
					continue;
				}
			
				_excludedPropertiesForUpdate.Add(propertyName);
			}
		}

		/// <summary>
		/// Gets the default exclusions for update. Warning: this is called during constructor, overrides need to be 
		/// sure to only return static values as to not cause issues.
		/// </summary>
		/// <returns> The values to exclude during update. </returns>
		public virtual HashSet<string> GetDefaultExclusionsForUpdate()
		{
			return new HashSet<string> { nameof(Id) };
		}

		/// <inheritdoc />
		public virtual bool IdIsSet()
		{
			return !Equals(Id, default(T));
		}

		/// <inheritdoc />
		public bool IsPropertyExcludedForUpdate(string propertyName)
		{
			return _excludedPropertiesForUpdate.Contains(propertyName);
		}

		/// <summary>
		/// Allows the entity to calculate the next key.
		/// </summary>
		/// <param name="currentKey"> The current version of the key. </param>
		/// <returns> The new key to be used in. </returns>
		public virtual T NewId(ref T currentKey)
		{
			switch (currentKey)
			{
				case sbyte sbKey:
					currentKey = (T) (object) (sbKey + 1);
					break;

				case byte bKey:
					currentKey = (T) (object) (bKey + 1);
					break;

				case short sKey:
					currentKey = (T) (object) (sKey + 1);
					break;

				case ushort usKey:
					currentKey = (T) (object) (usKey + 1);
					break;

				case int iKey:
					currentKey = (T) (object) (iKey + 1);
					break;

				case uint uiKey:
					currentKey = (T) (object) (uiKey + 1);
					break;

				case long lKey:
					currentKey = (T) (object) (lKey + 1);
					break;

				case ulong ulKey:
					currentKey = (T) (object) (ulKey + 1);
					break;
			}

			return currentKey;
		}

		/// <inheritdoc />
		public void ResetPropertyUpdateExclusions(bool setToDefault = true)
		{
			_excludedPropertiesForUpdate = setToDefault ? new HashSet<string>(GetDefaultExclusionsForUpdate()) : new HashSet<string>();
		}

		/// <inheritdoc />
		public virtual bool TrySetId(string id)
		{
			try
			{
				Id = id.FromJson<T>();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Unwrap the entity from the proxy as a specific type.
		/// </summary>
		/// <param name="update"> An optional update method. </param>
		/// <returns> The real entity unwrapped from the Entity Framework proxy. </returns>
		public T1 Unwrap<T1>(Action<T1> update = null)
		{
			return this.Unwrap<Entity<T>, T1>(update);
		}

		/// <summary>
		/// Unwrap the entity from the proxy.
		/// </summary>
		/// <returns>
		/// The real entity unwrapped from the Entity Framework proxy.
		/// </returns>
		public virtual object Unwrap()
		{
			var type = this.GetRealType();
			return this.Unwrap(type);
		}

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	public interface IEntity
	{
		#region Methods

		/// <summary>
		/// Checks to see if an entity can be modified.
		/// </summary>
		bool CanBeModified();

		/// <summary>
		/// Update an entity that has been added.
		/// </summary>
		void EntityAdded();

		/// <summary>
		/// Update an entity that has been deleted.
		/// </summary>
		void EntityDeleted();

		/// <summary>
		/// Update an entity that has been modified.
		/// </summary>
		void EntityModified();

		/// <summary>
		/// Add a property to exclude during update.
		/// </summary>
		/// <param name="propertyNames"> The names of the property to exclude. </param>
		void ExcludePropertiesForUpdate(params string[] propertyNames);

		/// <summary>
		/// Determine if the ID is set on the entity.
		/// </summary>
		/// <returns> True if the ID is set or false if otherwise. </returns>
		bool IdIsSet();

		/// <summary>
		/// Checks a property to see if it can be updated.
		/// </summary>
		/// <param name="propertyName"> The property name to be tested. </param>
		/// <returns> True if the property can be written during an update or false if otherwise. </returns>
		bool IsPropertyExcludedForUpdate(string propertyName);

		/// <summary>
		/// Resets exclusion back to default values or just clears if setToDefault is false.
		/// </summary>
		/// <param name="setToDefault"> Set to default excluded values. Defaults to true. </param>
		void ResetPropertyUpdateExclusions(bool setToDefault = true);

		/// <summary>
		/// Try to set the ID from a serialized version.
		/// </summary>
		/// <returns> True if the ID is successfully set or false if otherwise. </returns>
		bool TrySetId(string id);

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity that track the date and time it was created.
	/// </summary>
	public interface ICreatedEntity : IEntity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was created.
		/// </summary>
		DateTime CreatedOn { get; set; }

		#endregion
	}

	/// <summary>
	/// Represents a Speedy entity that track the date and time it was last modified.
	/// </summary>
	public interface IModifiableEntity : ICreatedEntity
	{
		#region Properties

		/// <summary>
		/// Gets or sets the date and time the entity was modified.
		/// </summary>
		DateTime ModifiedOn { get; set; }

		#endregion
	}
}