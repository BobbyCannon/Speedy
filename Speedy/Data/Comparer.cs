namespace Speedy.Data;

public abstract class Comparer<T> : StateComparer where T : new()
{
	#region Constructors

	protected Comparer()
	{
		CurrentState = new T();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public sealed override string TypeFullName => typeof(T).FullName;

	public T CurrentState { get; protected set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public sealed override object GetCurrentState()
	{
		return CurrentState;
	}

	public abstract bool ValidateUpdate(T update);

	/// <inheritdoc />
	public sealed override bool ValidateUpdate(object update)
	{
		return ValidateUpdate((T) update);
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

	protected abstract bool TryUpdateCurrentState(T update);

	protected override bool TryUpdateCurrentState(object update)
	{
		return TryUpdateCurrentState((T) update);
	}

	#endregion
}

public abstract class StateComparer
{
	#region Properties

	public abstract string TypeFullName { get; }

	#endregion

	#region Methods

	
	public abstract object GetCurrentState();
	
	public abstract bool ValidateUpdate(object state);

	public bool Refresh(object update)
	{
		if (!ValidateUpdate(update))
		{
			return false;
		}

		return TryUpdateCurrentState(update);
	}

	protected abstract bool TryUpdateCurrentState(object update);

	#endregion
}