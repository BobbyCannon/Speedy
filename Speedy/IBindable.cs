namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public interface IBindable
	{
		#region Methods

		/// <summary>
		/// Updates the entity for this entity.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		void UpdateDispatcher(IDispatcher dispatcher);

		#endregion
	}
}