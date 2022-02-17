#region References

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
		/// Creates an instance of the type.
		/// </summary>
		/// <returns> </returns>
		public override object GetInstance()
		{
			var response = Activator.CreateInstance<T>();
			return response;
		}

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
			ExcludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Validations = new Dictionary<string, PartialValidation>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Properties to be excluded.
		/// </summary>
		public HashSet<string> ExcludedProperties { get; }

		/// <summary>
		/// Properties to be included.
		/// </summary>
		public HashSet<string> IncludedProperties { get; }

		/// <summary>
		/// The validations for a partial update.
		/// </summary>
		public Dictionary<string, PartialValidation> Validations { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Creates an instance of the type.
		/// </summary>
		/// <returns> </returns>
		public abstract object GetInstance();

		#endregion
	}
}