namespace Speedy.Storage
{
	/// <summary>
	/// Represents an updatable item
	/// </summary>
	public interface IUpdatable
	{
		#region Methods

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="exclusions"> The properties will not be set during update. </param>
		void UpdateWith(object update, params string[] exclusions);

		#endregion
	}
}