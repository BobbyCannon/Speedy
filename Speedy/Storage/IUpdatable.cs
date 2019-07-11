namespace Speedy.Storage
{
	/// <summary>
	/// Represents an updatable item
	/// </summary>
	public interface IUpdatable<in T> : IUpdatable
	{
		#region Methods

		/// <summary>
		/// Update the entity with the value.
		/// </summary>
		/// <param name="value"> The value to update this object with. </param>
		void Update(T value);

		#endregion
	}

	/// <summary>
	/// Represents an updatable item
	/// </summary>
	public interface IUpdatable
	{
		#region Methods

		/// <summary>
		/// Update the entity with the value.
		/// </summary>
		/// <param name="value"> The value to update this object with. </param>
		void Update(object value);

		#endregion
	}
}