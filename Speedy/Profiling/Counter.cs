namespace Speedy.Profiling;

/// <summary>
/// Represents a counter for profiling.
/// </summary>
public class Counter : LockableBindable
{
	#region Fields

	private int _count;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiate the counter for profiling.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public Counter(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The current count value;
	/// </summary>
	public int Value => _count;

	#endregion

	#region Methods

	/// <summary>
	/// Decrement the counter by a value or default(1) if not provided.
	/// </summary>
	/// <param name="decrease"> The value to be decremented. </param>
	public void Decrement(int decrease = 1)
	{
		ThreadSafe.Decrement(ref _count, decrease);
		OnPropertyChanged(nameof(Value));
	}

	/// <summary>
	/// Increment the counter by a value or default(1) if not provided.
	/// </summary>
	/// <param name="increase"> The value to be incremented. </param>
	public void Increment(int increase = 1)
	{
		ThreadSafe.Increment(ref _count, increase);
		OnPropertyChanged(nameof(Value));
	}

	#endregion
}