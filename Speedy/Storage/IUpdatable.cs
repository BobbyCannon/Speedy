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
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		void UpdateWith(object update, params string[] exclusions);

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		void UpdateWith(object update, bool excludeVirtuals, params string[] exclusions);

		#endregion
	}

	/// <summary>
	/// Represents an updatable item
	/// </summary>
	/// <typeparam name="T"> The type that is update is for. </typeparam>
	public interface IUpdatable<in T> : IUpdatable
	{
		#region Methods

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <param name="update"> The source of the update. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		public void UpdateWith(T update, params string[] exclusions);

		#endregion
	}
}