#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Exceptions;
using Speedy.Storage;

#endregion

namespace Speedy.Configuration
{
	/// <summary>
	/// The configuration for an index property.
	/// </summary>
	public class IndexConfiguration
	{
		#region Fields

		private bool _isUnique;
		private readonly string _name;
		private readonly List<IPropertyConfiguration> _properties;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the index configuration.
		/// </summary>
		/// <param name="name"> The name of the index. </param>
		public IndexConfiguration(string name)
		{
			_name = name;
			_properties = new List<IPropertyConfiguration>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add property to the index configuration.
		/// </summary>
		/// <param name="property"> The property configuration to be added. </param>
		public void AddProperty(IPropertyConfiguration property)
		{
			_properties.Add(property);
		}

		/// <summary>
		/// Checks to see if this configuration is for the provided entity.
		/// </summary>
		/// <param name="entity"> The entity to test against. </param>
		/// <returns> True if this configuration is for the entity and false if otherwise. </returns>
		public bool IsMappingFor(object entity)
		{
			return _properties.Any(x => x.IsMappingFor(entity));
		}

		/// <summary>
		/// Marks the index as a unique.
		/// </summary>
		/// <returns> The configuration after updated. </returns>
		public IndexConfiguration IsUnique()
		{
			_isUnique = true;
			return this;
		}

		/// <summary>
		/// Validates the entity using this configuration.
		/// </summary>
		/// <param name="entity"> The entity to validate. </param>
		/// <param name="entityRepository"> The repository of entities. </param>
		public void Validate<T, T2>(object entity, IRepository<T, T2> entityRepository) where T : Entity<T2>
		{
			if (!(entity is T typedEntity))
			{
				throw new ArgumentNullException(nameof(typedEntity));
			}

			// Convert repository into local type so we can check new items
			var repository = (Repository<T, T2>) entityRepository;
			bool predicate(T x) => !ReferenceEquals(x, entity) && _properties.All(p => p.Matches(x, entity));
			var propertyName = string.Join("", _properties.Select(x => x.MemberName));

			if (propertyName == "SyncId" && Equals(_properties[0].GetValue(typedEntity), Guid.Empty))
			{
				return;
			}

			if (_isUnique && (repository.Any(predicate) || repository.AnyNew(entity, predicate)))
			{
			
				throw new ValidationException($"{_name}: Cannot insert duplicate row. The duplicate key value is ({string.Join(",", _properties.Select(x => x.GetValue(typedEntity)))}).");
			}
		}

		#endregion
	}
}