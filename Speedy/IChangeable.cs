#region References

using Speedy.Collections;

#endregion

namespace Speedy;

/// <summary>
/// Represents an object that can track changes for properties.
/// </summary>
public interface IChangeable
{
	#region Methods

	/// <summary>
	/// Get the list of changed properties.
	/// </summary>
	/// <returns> The list of changed property in a read only set. </returns>
	ReadOnlySet<string> GetChangedProperties();

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
	void ResetHasChanges();

	#endregion
}