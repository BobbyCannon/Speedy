namespace Speedy;

/// <summary>
/// Represents an unwrappable entity
/// </summary>
public interface IUnwrappable
{
	#region Methods

	/// <summary>
	/// Unwraps an object from the object proxy.
	/// </summary>
	/// <returns> The unwrapped entity. </returns>
	object Unwrap();

	#endregion
}