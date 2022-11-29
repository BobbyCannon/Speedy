namespace Speedy;

/// <summary>
/// Represents a comparer for a type.
/// </summary>
/// <typeparam name="T"> The type to be compared. </typeparam>
public abstract class Comparer<T> : Bindable, IComparer<T, T>
{
	#region Constructors

	/// <summary>
	/// Creates an instance of a comparer.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected Comparer(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="IComparer{T,T2,T3}.ShouldUpdate(T,T2)" />
	public abstract bool ShouldUpdate(T value, T update);

	/// <inheritdoc />
	public bool ShouldUpdate(object value, object update)
	{
		return value is T tValue
			&& update is T tUpdate
			&& ShouldUpdate(tValue, tUpdate);
	}

	/// <inheritdoc cref="IComparer{T,T2,T3}.UpdateWith(ref T,T3,string[])" />
	public abstract bool UpdateWith(ref T value, T update, params string[] exclusions);

	/// <inheritdoc />
	public bool UpdateWith(ref object value, object update, params string[] exclusions)
	{
		return value is T tValue
			&& update is T tUpdate
			&& UpdateWith(ref tValue, tUpdate, exclusions);
	}

	#endregion
}

/// <summary>
/// Represents a comparer for a type.
/// </summary>
/// <typeparam name="T"> The type to be compared. </typeparam>
/// <typeparam name="T2"> The type of the update. </typeparam>
public abstract class Comparer<T, T2> : Bindable, IComparer<T, T2>
{
	#region Constructors

	/// <summary>
	/// Creates an instance of a comparer.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected Comparer(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc cref="IComparer.ShouldUpdate" />
	public abstract bool ShouldUpdate(T value, T2 update);

	/// <inheritdoc />
	public bool ShouldUpdate(object value, object update)
	{
		return value is T tValue
			&& update is T tUpdate
			&& ShouldUpdate(tValue, tUpdate);
	}

	/// <inheritdoc cref="IComparer.UpdateWith" />
	public abstract bool UpdateWith(ref T value, T2 update, params string[] exclusions);

	/// <inheritdoc />
	public bool UpdateWith(ref object value, object update, params string[] exclusions)
	{
		return value is T tValue
			&& update is T2 tUpdate
			&& UpdateWith(ref tValue, tUpdate, exclusions);
	}

	#endregion
}

/// <summary>
/// Represents an interface to compare two instances of a type.
/// </summary>
public interface IComparer<T> : IComparer<T, T, T>
{
}

/// <summary>
/// Represents an interface to compare two instances of a type.
/// </summary>
public interface IComparer<T, in T2> : IComparer<T, T2, T2>
{
}

/// <summary>
/// Represents an interface to compare two instances of a type.
/// </summary>
public interface IComparer<T, in T2, in T3> : IComparer
{
	#region Methods

	/// <summary>
	/// Determine if the update should be applied.
	/// </summary>
	/// <param name="value"> The value to compare with. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldUpdate(T value, T2 update);

	/// <summary>
	/// Determine if the update should be applied.
	/// </summary>
	/// <param name="value"> The value to compare with. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldUpdate(T value, T3 update);

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool UpdateWith(ref T value, T2 update, params string[] exclusions);

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool UpdateWith(ref T value, T3 update, params string[] exclusions);

	#endregion
}

/// <summary>
/// Represents an interface to compare two instances of a type.
/// </summary>
public interface IComparer
{
	#region Methods

	/// <summary>
	/// Determine if the update should be applied.
	/// </summary>
	/// <param name="value"> The value to compare with. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldUpdate(object value, object update);

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool UpdateWith(ref object value, object update, params string[] exclusions);

	#endregion
}