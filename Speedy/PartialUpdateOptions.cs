#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;
using Speedy.Validation;

#endregion

namespace Speedy
{
	/// <summary>
	/// Options for Partial Update
	/// </summary>
	public class PartialUpdateOptions<T> : PartialUpdateOptions
	{
		#region Constructors

		/// <summary>
		/// Create an instance of the partial update options.
		/// </summary>
		public PartialUpdateOptions()
		{
			Validator = new Validator<T>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Validator for validating the options for the type.
		/// </summary>
		public Validator<T> Validator { get; }

		#endregion

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

		#endregion

		#region Methods

		/// <summary>
		/// Create options for the provided type.
		/// </summary>
		/// <param name="type"> </param>
		/// <returns> The partial update options for the provided type. </returns>
		public static PartialUpdateOptions Create(Type type)
		{
			return (PartialUpdateOptions) typeof(PartialUpdateOptions<>).CreateNewGenericInstance(new[] { type });
		}

		/// <summary>
		/// Creates an instance of the type.
		/// </summary>
		/// <returns> </returns>
		public abstract object GetInstance();

		/// <summary>
		/// Check to see if a property should be processed.
		/// </summary>
		/// <param name="propertyName"> The name of the property to test. </param>
		/// <returns> True if the property should be processed otherwise false. </returns>
		public bool ShouldProcessProperty(string propertyName)
		{
			if (IncludedProperties.Any()
				&& !IncludedProperties.Contains(propertyName))
			{
				// Ignore this property because we only want to include it
				return false;
			}

			if (ExcludedProperties.Contains(propertyName))
			{
				// Ignore this property because we only want to exclude it
				return false;
			}

			return true;
		}

		#endregion
	}
}