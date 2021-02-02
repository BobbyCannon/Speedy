namespace Speedy.Serialization
{
	/// <summary>
	/// Represents a cloneable item.
	/// </summary>
	/// <typeparam name="T"> The type of the item to clone. </typeparam>
	public interface ICloneable<out T> : ICloneable
	{
		#region Methods

		/// <summary>
		/// Deep clone the item with child relationships. Default level is -1 which means clone full hierarchy of children.
		/// </summary>
		/// <param name="levels"> The number of levels deep to clone. Default is full hierarchy. </param>
		/// <returns> The cloned objects. </returns>
		public new T DeepClone(int levels = -1);

		/// <summary>
		/// Shallow clone the item. No child items are cloned.
		/// </summary>
		/// <returns> The cloned objects. </returns>
		public new T ShallowClone();

		/// <summary>
		/// Shallow clone the item. No child items are cloned.
		/// </summary>
		/// <param name="exclusions"> The properties will not be set during the clone. </param>
		/// <returns> The cloned objects. </returns>
		public new T ShallowCloneExcept(params string[] exclusions);

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
		/// <param name="levels"> The number of levels deep to clone. Default is full hierarchy. </param>
		/// <returns> The cloned objects. </returns>
		public object DeepClone(int levels = -1);

		/// <summary>
		/// Shallow clone the item. No child items are cloned.
		/// </summary>
		/// <returns> The cloned objects. </returns>
		public object ShallowClone();

		/// <summary>
		/// Shallow clone the item. No child items are cloned.
		/// </summary>
		/// <param name="exclusions"> The properties will not be set during the clone. </param>
		/// <returns> The cloned objects. </returns>
		public object ShallowCloneExcept(params string[] exclusions);

		#endregion
	}
}