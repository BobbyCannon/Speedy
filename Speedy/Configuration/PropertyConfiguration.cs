#region References

using System;
using System.Linq.Expressions;
using Speedy.Exceptions;

#endregion

namespace Speedy.Configuration
{
	/// <summary>
	/// The configuration for an entity property.
	/// </summary>
	/// <typeparam name="T"> The entity this configuration is for. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	public class PropertyConfiguration<T, T2> : IPropertyConfiguration where T : Entity<T2>
	{
		#region Fields

		private readonly Type _entityType;
		private int _maxLength;
		private int _minLength;
		private readonly string _nodeType;
		private readonly Expression<Func<T, object>> _property;
		private readonly Func<T, object> _propertyFunction;
		private string _typeName;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the property configuration.
		/// </summary>
		/// <param name="property"> The property expression this configuration is for. </param>
		public PropertyConfiguration(Expression<Func<T, object>> property)
		{
			_entityType = typeof(T);
			_property = property;
			_nodeType = ((dynamic) _property).Body.NodeType.ToString();
			_propertyFunction = _property.Compile();
			_maxLength = -1;
			_minLength = -1;

			IsNullable = null;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public RelationshipDeleteBehavior DeleteBehavior { get; private set; }

		/// <inheritdoc />
		public bool? IsNullable { get; set; }

		/// <inheritdoc />
		public string MemberName { get; set; }

		/// <inheritdoc />
		public string TypeName => _typeName ??= typeof(T).Name;

		#endregion

		#region Methods

		/// <inheritdoc />
		public object GetValue(object entity)
		{
			if (!(entity is T typedEntity))
			{
				throw new ArgumentNullException(nameof(typedEntity));
			}

			return _propertyFunction.Invoke(typedEntity);
		}

		/// <summary>
		/// Sets the maximum length of the member.
		/// </summary>
		/// <returns> The configuration after updated. </returns>
		public PropertyConfiguration<T, T2> HasMaximumLength(int length)
		{
			_maxLength = length;
			return this;
		}

		/// <summary>
		/// Sets the minimum length of the member.
		/// </summary>
		/// <returns> The configuration after updated. </returns>
		public PropertyConfiguration<T, T2> HasMinimumLength(int length)
		{
			_minLength = length;
			return this;
		}

		/// <summary>
		/// Checks to see if this configuration is for the provided entity.
		/// </summary>
		/// <param name="entity"> The entity to test against. </param>
		/// <returns> True if this configuration is for the entity and false if otherwise. </returns>
		public bool IsMappingFor(object entity)
		{
			return entity is T;
		}

		/// <summary>
		/// Marks the property as a required member.
		/// </summary>
		/// <param name="required"> The value to determine if the property is required. </param>
		/// <returns> The configuration after updated. </returns>
		public PropertyConfiguration<T, T2> IsRequired(bool required = true)
		{
			IsNullable = !required;
			return this;
		}

		/// <inheritdoc />
		public bool Matches(object object1, object object2)
		{
			if (!(object1 is T typedEntity1))
			{
				throw new ArgumentNullException(nameof(typedEntity1));
			}

			if (!(object2 is T typedEntity2))
			{
				throw new ArgumentNullException(nameof(typedEntity2));
			}

			var property1 = _propertyFunction.Invoke(typedEntity1);
			var property2 = _propertyFunction.Invoke(typedEntity2);

			return Equals(property1, property2);
		}

		/// <inheritdoc />
		public void OnDelete(RelationshipDeleteBehavior behavior)
		{
			DeleteBehavior = behavior;
		}

		/// <summary>
		/// Validates the entity using this configuration.
		/// </summary>
		/// <param name="entity"> The entity to validate. </param>
		/// <param name="entityRepository"> The repository of entities. </param>
		public void Validate(object entity, IRepository<T, T2> entityRepository)
		{
			if (!(entity is T typedEntity))
			{
				throw new ArgumentNullException(nameof(typedEntity));
			}

			if (_nodeType != "Convert" && _nodeType != "MemberAccess")
			{
				return;
			}

			var property = _propertyFunction.Invoke(typedEntity);

			if (IsNullable.HasValue && IsNullable.Value == false && property == null)
			{
				throw new ValidationException($"{_entityType.Name}: The {MemberName} field is required.");
			}

			if ((_maxLength > 0 || _minLength > 0) && property is string stringEntity)
			{
				if (_maxLength > 0 && stringEntity.Length > _maxLength)
				{
					throw new ValidationException($"{_entityType.Name}: The {MemberName} field is too long.");
				}

				if (_minLength > 0 && stringEntity.Length < _minLength)
				{
					throw new ValidationException($"{_entityType.Name}: The {MemberName} field is too short.");
				}
			}

			if (IsNullable.HasValue && IsNullable.Value == false && property == null)
			{
				throw new ValidationException($"{_entityType.Name}: The {MemberName} field is required.");
			}
		}

		/// <inheritdoc />
		IPropertyConfiguration IPropertyConfiguration.HasMaximumLength(int maxLength)
		{
			return HasMaximumLength(maxLength);
		}

		#endregion
	}
}