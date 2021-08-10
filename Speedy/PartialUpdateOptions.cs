#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Speedy.Exceptions;
using Speedy.Extensions;

#endregion

namespace Speedy
{
	/// <summary>
	/// Options for Partial Update
	/// </summary>
	public class PartialUpdateOptions<T> : PartialUpdateOptions
	{
		#region Methods

		/// <summary>
		/// Configure a validation for a property.
		/// </summary>
		public PartialValidation<TProperty> Property<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			var name = propertyExpression.Member.Name;
			var response = new PartialValidation<TProperty>(name);
			Validations.Add(name, response);
			return response;
		}

		#endregion
	}

	/// <summary>
	/// Options for Partial Update
	/// </summary>
	public abstract class PartialUpdateOptions
	{
		#region Constructors

		/// <summary>
		/// Instantiates options for validation for a partial update.
		/// </summary>
		protected PartialUpdateOptions()
		{
			Validations = new Dictionary<string, PartialValidation>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Properties to be excluded.
		/// </summary>
		public string[] ExcludedProperties { get; private set; }

		/// <summary>
		/// Properties to be included.
		/// </summary>
		public string[] IncludedProperties { get; private set; }

		/// <summary>
		/// The validations for a partial update.
		/// </summary>
		public Dictionary<string, PartialValidation> Validations { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Exclude properties for this partial update options.
		/// </summary>
		/// <param name="properties"> The properties to be excluded. </param>
		public void ExcludeProperties(params string[] properties)
		{
			ExcludedProperties = properties;
		}

		/// <summary>
		/// Include properties for this partial update options.
		/// </summary>
		/// <param name="properties"> The properties to be included. </param>
		public void IncludeProperties(params string[] properties)
		{
			IncludedProperties = properties;
		}

		internal static PartialUpdateOptions Create(Type genericType)
		{
			var partialType = typeof(PartialUpdateOptions<>);
			var typeWithGeneric = partialType.MakeGenericType(genericType);
			var response = (PartialUpdateOptions) Activator.CreateInstance(typeWithGeneric);
			return response;
		}

		#endregion

		#region Classes

		/// <summary>
		/// Validation for Partial Update
		/// </summary>
		public abstract class PartialValidation
		{
			#region Constructors

			/// <summary>
			/// Instantiates a validation for a partial update.
			/// </summary>
			protected PartialValidation(string name)
			{
				Name = name;
			}

			#endregion

			#region Properties

			/// <summary>
			/// The text of the message that will be thrown if the validation fails.
			/// </summary>
			public string Message { get; protected set; }

			/// <summary>
			/// The name of the validation.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// Gets or sets the flag to clear / set a required requirement.
			/// </summary>
			public bool Required { get; protected set; }

			#endregion

			#region Methods

			/// <summary>
			/// Process a value through an update validation.
			/// </summary>
			/// <param name="value"> The value to test against a validation. </param>
			public abstract void Process(object value);

			#endregion
		}

		/// <summary>
		/// Validation for Partial Update
		/// </summary>
		public class PartialValidation<T> : PartialValidation
		{
			#region Fields

			private Func<T, bool> _validate;

			#endregion

			#region Constructors

			/// <summary>
			/// Instantiates a validation for a partial update.
			/// </summary>
			/// <param name="name"> The name of the partial update. </param>
			public PartialValidation(string name) : base(name)
			{
			}

			#endregion

			#region Methods

			/// <summary>
			/// Set a minimum and maximum for the value.
			/// </summary>
			/// <param name="minimum"> The inclusive minimal value. </param>
			/// <param name="maximum"> The inclusive maximum value. </param>
			/// <returns> The validation for method chaining. </returns>
			public PartialValidation<T> HasMinMaxRange(int minimum, int maximum)
			{
				if (typeof(T) == typeof(string))
				{
					_validate = x => x.ValidateStringRange(minimum, maximum);
				}
				else if (typeof(T) == typeof(int))
				{
					_validate = x => x.ValidateIntRange(minimum, maximum);
				}

				return this;
			}

			/// <summary>
			/// Sets the flag to clear / set a required requirement.
			/// </summary>
			/// <param name="required"> </param>
			/// <returns> The validation for method chaining. </returns>
			public PartialValidation<T> IsRequired(bool required = true)
			{
				Required = required;
				return this;
			}

			/// <summary>
			/// Process a validation against an object.
			/// </summary>
			/// <param name="value"> The object to validate against. Must be of type T. </param>
			public override void Process(object value)
			{
				Process((T) value);
			}

			/// <summary>
			/// Process a validation against an object.
			/// </summary>
			/// <param name="value"> The value to validate against. </param>
			public virtual void Process(T value)
			{
				if (!_validate.Invoke(value))
				{
					throw new ValidationException(Message);
				}
			}

			/// <summary>
			/// The message to throw if the validation fails.
			/// </summary>
			/// <param name="message"> The text of the message that will be thrown if the validation fails. </param>
			/// <returns> The validation for method chaining. </returns>
			public PartialValidation<T> Throws(string message)
			{
				Message = message;
				return this;
			}

			#endregion
		}

		#endregion
	}
}