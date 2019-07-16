namespace Speedy.Configuration
{
	/// <summary>
	/// The delete behavior of relationships
	/// </summary>
	public enum RelationshipDeleteBehavior
	{
		/// <summary>
		/// The values of foreign key properties in dependent entities are not changed.
		/// </summary>
		Restrict,

		/// <summary>
		/// The dependent entities will also be deleted.
		/// </summary>
		Cascade,

		/// <summary>
		/// The values of foreign key properties in dependent entities are set to null.
		/// </summary>
		SetNull
	}
}