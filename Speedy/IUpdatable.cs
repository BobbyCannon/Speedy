namespace Speedy;

/// <summary>
/// Represents an updatable item
/// </summary>
/// <typeparam name="T"> The type that is update is for. </typeparam>
public interface IUpdatable<in T> : IUpdatable
{
	#region Methods

	/// <summary>
	/// Determine if the update should be applied.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldUpdate(T update);

	/// <summary>
	/// Determine if the update should be applied then applies it if so else do nothing.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool TryUpdateWith(T update, params string[] exclusions);

	/// <summary>
	/// Allows updating of one type to another based on member Name and Type.
	/// </summary>
	/// <param name="update"> The source of the update. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool UpdateWith(T update, params string[] exclusions);

	#endregion
}

/// <summary>
/// Represents an updatable item
/// </summary>
public interface IUpdatable
{
	#region Methods

	/// <summary>
	/// Determine if the update should be applied.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldUpdate(object update);

	/// <summary>
	/// Determine if the update should be applied then applies it if so else do nothing.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool TryUpdateWith(object update, params string[] exclusions);

	/// <summary>
	/// Allows updating of one type to another based on member Name and Type.
	/// </summary>
	/// <param name="update"> The source of the update. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	bool UpdateWith(object update, params string[] exclusions);

	/// <summary>
	/// Allows updating of one type to another based on member Name and Type.
	/// </summary>
	/// <param name="update"> The source of the update. </param>
	/// <param name="excludeVirtuals"> An optional value to exclude virtual members. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	bool UpdateWith(object update, bool excludeVirtuals, params string[] exclusions);

	#endregion
}