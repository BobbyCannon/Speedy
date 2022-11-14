namespace Speedy.Data;

/// <summary>
/// The comparer for an object.
/// </summary>
public abstract class Comparer<T> : Comparer, IComparer<T>
	where T : new()
{
	#region Fields

	private T _currentValue;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a new comparer.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected Comparer(IDispatcher dispatcher) : base(dispatcher)
	{
		CurrentValue = new T();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The value of the comparer.
	/// </summary>
	public T CurrentValue
	{
		get => _currentValue;
		protected set => _currentValue = value;
	}

	/// <inheritdoc />
	public sealed override string TypeFullName => typeof(T).FullName;

	#endregion

	#region Methods

	/// <inheritdoc />
	public sealed override object GetCurrentValue()
	{
		return CurrentValue;
	}

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool Refresh(ref T value, T update)
	{
		return ShouldApplyUpdate(value, update)
			&& TryUpdateValue(ref value, update);
	}

	/// <inheritdoc />
	public override bool ShouldApplyUpdate(object update)
	{
		return ShouldApplyUpdate(_currentValue, (T) update);
	}

	/// <inheritdoc />
	public override bool ShouldApplyUpdate(object value, object update)
	{
		return value is T tValue 
			&& ShouldApplyUpdate(tValue, (T) update);
	}

	/// <summary>
	/// Determine if the update should be applied to the current value.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public bool ShouldApplyUpdate(T update)
	{
		return ShouldApplyUpdate(_currentValue, update);
	}

	/// <summary>
	/// Determine if the update should be applied to the provided value.
	/// </summary>
	/// <param name="value"> The current value state. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public abstract bool ShouldApplyUpdate(T value, T update);

	/// <inheritdoc />
	public sealed override bool TryUpdateValue(object update)
	{
		return TryUpdateValue((T) update);
	}

	/// <inheritdoc />
	public override bool TryUpdateValue(ref object value, object update)
	{
		return value is T tValue
			&& TryUpdateValue(ref tValue, (T) update);
	}

	/// <summary>
	/// Apply the update to the current value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool TryUpdateValue(T update)
	{
		return TryUpdateValue(ref _currentValue, update);
	}

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public abstract bool TryUpdateValue(ref T value, T update);

	#endregion
}

/// <summary>
/// The comparer for an object.
/// </summary>
public abstract class Comparer : Bindable, IComparer
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of a comparer for an object.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected Comparer(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The full name of the type this comparer is for.
	/// </summary>
	public abstract string TypeFullName { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Get the value.
	/// </summary>
	/// <returns> The value of the comparer. </returns>
	public abstract object GetCurrentValue();

	/// <summary>
	/// Try to refresh the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	public bool Refresh(object update)
	{
		return ShouldApplyUpdate(update)
			&& TryUpdateValue(update);
	}

	/// <inheritdoc />
	public bool Refresh(ref object value, object update)
	{
		return ShouldApplyUpdate(value, update)
			&& TryUpdateValue(ref value, update);
	}

	/// <inheritdoc />
	public abstract bool ShouldApplyUpdate(object update);

	/// <inheritdoc />
	public abstract bool ShouldApplyUpdate(object value, object update);

	/// <inheritdoc />
	public abstract bool TryUpdateValue(object update);

	/// <inheritdoc />
	public abstract bool TryUpdateValue(ref object value, object update);

	#endregion
}

/// <summary>
/// The comparer for an object.
/// </summary>
public interface IComparer<out T> : IComparer
{
	#region Properties

	/// <summary>
	/// The value of the comparer.
	/// </summary>
	public T CurrentValue { get; }

	#endregion
}

/// <summary>
/// The comparer for an object.
/// </summary>
public interface IComparer : IBindable
{
	#region Properties

	/// <summary>
	/// The full name of the type this comparer is for.
	/// </summary>
	string TypeFullName { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Get the value.
	/// </summary>
	/// <returns> The value of the comparer. </returns>
	object GetCurrentValue();

	/// <summary>
	/// Try to refresh the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	bool Refresh(object update);

	/// <summary>
	/// Try to refresh the value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(ref object value, object update);

	/// <summary>
	/// Determine if the update should be applied to the current value.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldApplyUpdate(object update);

	/// <summary>
	/// Determine if the update should be applied to the provided value.
	/// </summary>
	/// <param name="value"> The current value state. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	bool ShouldApplyUpdate(object value, object update);

	/// <summary>
	/// Apply the update to the current value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool TryUpdateValue(object update);

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	abstract bool TryUpdateValue(ref object value, object update);

	#endregion
}