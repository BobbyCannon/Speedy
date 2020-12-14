namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public interface IBindable
	{
		#region Methods

		/// <summary>
		/// Get the current dispatcher in use.
		/// </summary>
		/// <returns>
		/// The dispatcher that is currently being used. Null if no dispatcher is assigned.
		/// </returns>
		IDispatcher GetDispatcher();

		/// <summary>
		/// Updates the entity for this entity.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		void UpdateDispatcher(IDispatcher dispatcher);

		#endregion
	}
}