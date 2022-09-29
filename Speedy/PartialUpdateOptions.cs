#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy
{
	/// <summary>
	/// Options for Partial Update
	/// </summary>
	public class PartialUpdateOptions : Bindable, ICloneable
	{
		#region Constructors

		/// <summary>
		/// Instantiates options for validation for a partial update.
		/// </summary>
		public PartialUpdateOptions() : this(null, null)
		{
		}

		/// <summary>
		/// Instantiates options for validation for a partial update.
		/// </summary>
		public PartialUpdateOptions(IEnumerable<string> including, IEnumerable<string> excluding)
		{
			ExcludedProperties = excluding != null
				? new HashSet<string>(excluding, StringComparer.OrdinalIgnoreCase)
				: new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedProperties = including != null
				? new HashSet<string>(including, StringComparer.OrdinalIgnoreCase)
				: new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

		/// <inheritdoc />
		public object DeepClone(int? maxDepth = null)
		{
			return ShallowClone();
		}

		/// <inheritdoc />
		public object ShallowClone()
		{
			return new PartialUpdateOptions(IncludedProperties, ExcludedProperties);
		}

		/// <summary>
		/// Check to see if a property should be processed.
		/// </summary>
		/// <param name="propertyName"> The name of the property to test. </param>
		/// <returns> True if the property should be processed otherwise false. </returns>
		public bool ShouldProcessProperty(string propertyName)
		{
			if (IncludedProperties.Any() && !IncludedProperties.Contains(propertyName))
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

		/// <summary>
		/// Update the PartialUpdateOptions with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public void UpdateWith(PartialUpdateOptions update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				ExcludedProperties.Reconcile(update.ExcludedProperties);
				IncludedProperties.Reconcile(update.IncludedProperties);
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(ExcludedProperties)), x => x.ExcludedProperties.Reconcile(update.ExcludedProperties));
				this.IfThen(_ => !exclusions.Contains(nameof(IncludedProperties)), x => x.IncludedProperties.Reconcile(update.IncludedProperties));
			}

			//base.UpdateWith(update, exclusions);
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case PartialUpdateOptions options:
				{
					UpdateWith(options, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
		}

		#endregion
	}
}