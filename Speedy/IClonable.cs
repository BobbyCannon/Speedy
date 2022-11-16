namespace Speedy;

/// <summary>
/// Represents a cloneable item.
/// </summary>
public interface ICloneable<out T> : ICloneable
{
	#region Methods

	/// <summary>
	/// Deep clone the item with child relationships. Default level is -1 which means clone full hierarchy of children.
	/// </summary>
	/// <param name="maxDepth"> The max depth to clone. Defaults to null. </param>
	/// <returns> The cloned objects. </returns>
	new T DeepClone(int? maxDepth = null);

	/// <summary>
	/// Shallow clone the item. No child items are cloned.
	/// </summary>
	/// <returns> The cloned objects. </returns>
	new T ShallowClone();

	#endregion
}

/// <summary>
/// Represents a cloneable item.
/// </summary>
public interface ICloneable
{
	#region Methods

	/// <summary>
	/// Deep clone the item with child relationships. Default level is -1 which means clone full hierarchy of children.
	/// </summary>
	/// <param name="maxDepth"> The max depth to clone. Defaults to null. </param>
	/// <returns> The cloned objects. </returns>
	public object DeepClone(int? maxDepth = null);

	/// <summary>
	/// Shallow clone the item. No child items are cloned.
	/// </summary>
	/// <returns> The cloned objects. </returns>
	public object ShallowClone();

	#endregion
}