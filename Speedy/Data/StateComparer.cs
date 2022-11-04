namespace Speedy.Data;

public abstract class StateComparer<T> : StateComparer where T : new()
{
	#region Constructors

	protected StateComparer()
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

	public abstract bool ShouldUpdateState(T update);

	/// <inheritdoc />
	public sealed override bool IsBetter(object update)
	{
		return ShouldUpdateState((T) update);
	}

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
	
	public abstract bool IsBetter(object state);

	public bool Refresh(object update)
	{
		if (!IsBetter(update))
		{
			return false;
		}

		return TryUpdateCurrentState(update);
	}

	protected abstract bool TryUpdateCurrentState(object update);

	#endregion
}