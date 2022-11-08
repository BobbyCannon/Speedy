#region References

using System.Collections.Concurrent;

#endregion

namespace Speedy.Data;

/// <summary>
/// Manages a set of comparers.
/// </summary>
public class ComparerManager : Bindable
{
	#region Fields

	private readonly ConcurrentDictionary<string, Comparer> _stateComparers;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the manager for comparers.
	/// </summary>
	public ComparerManager()
	{
		_stateComparers = new ConcurrentDictionary<string, Comparer>();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Add a comparer to the manager.
	/// </summary>
	/// <param name="comparer"> The comparer to be added. </param>
	public void AddComparer(Comparer comparer)
	{
		_stateComparers.AddOrUpdate(comparer.TypeFullName, x => comparer, (x, c) => comparer);
	}

	/// <summary>
	/// Get the value of a type from the loaded comparer.
	/// </summary>
	/// <typeparam name="T"> The type to get the value for. </typeparam>
	/// <returns> The type if a comparer was found otherwise default(T). </returns>
	public T GetValue<T>()
	{
		var type = typeof(T);

		if ((type.FullName == null) || !_stateComparers.TryGetValue(type.FullName, out var comparer))
		{
			return default;
		}

		var currentState = comparer.GetValue();
		return currentState is T state ? state : default;
	}

	/// <summary>
	/// Try to refresh a comparer with an update.
	/// </summary>
	/// <typeparam name="T"> The type the update is for. </typeparam>
	/// <param name="update"> The update to try and refresh a comparer with. </param>
	/// <returns> True if the comparer was found and refreshed otherwise false. </returns>
	public bool TryRefresh<T>(T update)
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