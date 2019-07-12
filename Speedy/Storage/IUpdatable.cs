namespace Speedy.Storage
{
	/// <summary>
	/// Represents an updatable item
	/// </summary>
	public interface IUpdatable<in T> : IUpdatable
	{
		#region Methods

		/// <summary>
		/// Update the entity with the provided value.
		/// </summary>
		/// <param name="value"> The value to update this object with. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. Defaults to true. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		void UpdateWith(T value, bool excludeVirtuals = true, params string[] exclusions);

		#endregion
	}

	/// <summary>
	/// Represents an updatable item
	/// </summary>
	public interface IUpdatable
	{
		#region Methods

		/// <summary>
		/// Update the entity with the provided value.
		/// </summary>
		/// <param name="value"> The value to update this object with. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. Defaults to true. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		void UpdateWith(object value, bool excludeVirtuals = true, params string[] exclusions);

		#endregion
	}
}