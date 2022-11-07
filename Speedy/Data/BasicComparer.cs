using System;

namespace Speedy.Data;

public class BasicComparer<T> : Comparer<T> where T : new()
{
	#region Fields

	private readonly Func<T, T, bool> _isBetter;
	private readonly Func<T, T, (T, bool)> _update;

	#endregion

	#region Constructors

	public BasicComparer(Func<T, T, bool> isBetter, Func<T, T, (T, bool)> update)
	{
		_isBetter = isBetter;
		_update = update;
	}

	#endregion

	#region Methods

	public override bool ValidateUpdate(T state)
	{
		return _isBetter.Invoke(CurrentState, state);
	}

	protected override bool TryUpdateCurrentState(T update)
	{
		var result = _update.Invoke(CurrentState, update);
		if (result.Item2)
		{
			CurrentState = result.Item1;
		}

		return result.Item2;
	}

	#endregion
}