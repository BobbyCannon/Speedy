namespace Speedy;

/// <summary>
/// Represents an object that can track changes for properties.
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
	/// Reset the "has changes" state.
	/// </summary>
	void ResetHasChanges();

	#endregion
}