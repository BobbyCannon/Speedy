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

		/// <summary>
		/// Gets the member name of the property this configuration is for.
		/// </summary>
		string MemberName { get; set; }

		/// <summary>
		/// Gets the type name of the property this configuration is for.
		/// </summary>
		string TypeName { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Get the value for the property configuration.
		/// </summary>
		/// <param name="entity"> The entity to get the value of. </param>
		/// <returns> The value in string format. </returns>
		object GetValue(object entity);

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
		/// Checks two objects to see if the properties match
		/// </summary>
		/// <param name="object1"> The first object. </param>
		/// <param name="object2"> The second object. </param>
		/// <returns> </returns>
		bool Matches(object object1, object object2);

		/// <summary>
		/// </summary>
		/// <param name="behavior"> The delete behavior for the foreign key relationship. </param>
		void OnDelete(RelationshipDeleteBehavior behavior);

		#endregion
	}
}