#region References

using System.Collections.Concurrent;

#endregion

namespace Speedy.Data;

/// <summary>
/// Manages different state
/// </summary>
public class StateManager : Bindable
{
	#region Fields

	private readonly ConcurrentDictionary<string, StateComparer> _stateComparers;

	#endregion

	#region Constructors

	public StateManager()
	{
		_stateComparers = new ConcurrentDictionary<string, StateComparer>();
	}

	#endregion

	#region Methods

	public void AddComparer(StateComparer comparer)
	{
		_stateComparers.AddOrUpdate(comparer.TypeFullName, x => comparer, (x, c) => comparer);
	}

	public T GetCurrentState<T>()
	{
		var type = typeof(T);

		if ((type.FullName == null) || !_stateComparers.TryGetValue(type.FullName, out var comparer))
		{
			return default;
		}

		var currentState = comparer.GetCurrentState();
		return currentState is T state ? state : default;
	}

	public bool TryAddOrUpdateState<T>(T update)
	{
		if (update == null)
		{
			return false;
		}

		var type = typeof(T);

		if ((type.FullName == null) || !_stateComparers.TryGetValue(type.FullName, out var comparer))
		{
			return false;
		}

		return comparer.Refresh(update);
	}

	#endregion
}