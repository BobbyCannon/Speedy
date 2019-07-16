#region References

using System.Linq;

#endregion

namespace Speedy.Configuration
{
	/// <summary>
	/// The interface for the property configuration.
	/// </summary>
	public interface IPropertyConfiguration
	{
		#region Properties

		/// <summary>
		/// The delete behavior for this property. Only applies to foreign keys.
		/// </summary>
		RelationshipDeleteBehavior DeleteBehavior { get; }

		/// <summary>
		/// Indicates this property can be set to null.
		/// </summary>
		bool? IsNullable { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Sets the max length for this property.
		/// </summary>
		/// <param name="maxLength"> The max length for the property. </param>
		/// <returns> The modified property configuration. </returns>
		IPropertyConfiguration HasMaximumLength(int maxLength);

		/// <summary>
		/// Checks to see if this configuration is for the provided entity.
		/// </summary>
		/// <param name="entity"> The entity to test against. </param>
		/// <returns> True if this configuration is for the entity and false if otherwise. </returns>
		bool IsMappingFor(object entity);

		/// <summary>
		/// Marks the property as a unique member.
		/// </summary>
		/// <returns> The configuration after updated. </returns>
		IPropertyConfiguration IsUnique();

		/// <summary>
		/// </summary>
		/// <param name="behavior"> The delete behavior for the foreign key relationship. </param>
		void OnDelete(RelationshipDeleteBehavior behavior);

		/// <summary>
		/// Validates the entity using this configuration.
		/// </summary>
		/// <param name="entity"> The entity to validate. </param>
		/// <param name="repository"> The entity repository. </param>
		void Validate(object entity, IQueryable repository);

		#endregion
	}
}