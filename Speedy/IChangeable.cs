namespace Speedy;

/// <summary>
/// Represents an object that can track changes
/// </summary>
public interface IChangeable
{
	#region Methods

	/// <summary>
	/// Determines if the object has changes.
	/// </summary>
	/// <returns> True if the object has changes otherwise false. </returns>
	bool HasChanges();

	/// <summary>
	/// Determines if the object has changes.
	/// </summary>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	/// <returns> True if the object has changes otherwise false. </returns>
	bool HasChanges(params string[] exclusions);

	/// <summary>
	/// Reset the "has changes" state.
	/// </summary>
	/// <param name="hasChanges"> An optional value to indicate if this object has changes. Defaults to false. </param>
	void ResetHasChanges(bool hasChanges = false);

	#endregion
}