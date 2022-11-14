namespace Speedy.Data;

/// <inheritdoc />
public abstract class Comparer<T> : Comparer where T : new()
{
	#region Constructors

	/// <summary>
	/// Instantiates a new comparer.
	/// </summary>
	protected Comparer()
	{
		Value = new T();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public sealed override string TypeFullName => typeof(T).FullName;

	/// <summary>
	/// The value of the comparer.
	/// </summary>
	public T Value { get; protected set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public sealed override object GetValue()
	{
		return Value;
	}

	/// <summary>
	/// Refresh the comparer with an update.
	/// </summary>
	/// <param name="update"> The update to be checked and optionally applied. </param>
	/// <returns> True if the update was accepted and applied otherwise false. </returns>
	/// <remarks> Note: the update could and may have been partially applied. </remarks>
	public bool Refresh(T update)
	{
		return base.Refresh(update);
	}

	/// <summary>
	/// Validates an update to see if it should be applied.
	/// </summary>
	/// <param name="update"> The update to validate. </param>
	/// <returns> True if the update should be applied. </returns>
	public abstract bool ValidateUpdate(T update);

	/// <inheritdoc />
	public sealed override bool ValidateUpdate(object update)
	{
		return ValidateUpdate((T) update);
	}

	/// <summary>
	/// Try to update the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	protected abstract bool TryUpdateValue(T update);

	/// <inheritdoc />
	protected sealed override bool TryUpdateValue(object update)
	{
		return TryUpdateValue((T) update);
	}

	#endregion
}

/// <summary>
/// The comparer for an object.
/// </summary>
public abstract class Comparer : Bindable
{
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
	public abstract object GetValue();

	/// <summary>
	/// Try to refresh the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	public bool Refresh(object update)
	{
		if (!ValidateUpdate(update))
		{
			return false;
		}

		return TryUpdateValue(update);
	}

	/// <summary>
	/// Validate an update.
	/// </summary>
	/// <param name="update"> The update to validate. </param>
	/// <returns> True if the update is valid otherwise false. </returns>
	public abstract bool ValidateUpdate(object update);

	/// <summary>
	/// Try to update the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	protected abstract bool TryUpdateValue(object update);

	#endregion
}