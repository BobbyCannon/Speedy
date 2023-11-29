#region References

using Speedy.Profiling;

#endregion

namespace Speedy.Collections;

/// <inheritdoc />
public class SpeedyListProfiler : ProfilerService
{
	#region Constructors

	/// <inheritdoc />
	public SpeedyListProfiler(IDispatcher dispatcher) : base(dispatcher)
	{
		AddedCount = new Counter(null, dispatcher);
		OrderCount = new Counter(null, dispatcher);
		RemovedCount = new Counter(null, dispatcher);
	}

	#endregion

	#region Properties

	/// <summary>
	/// The number of items added to the list.
	/// </summary>
	public Counter AddedCount { get; }

	/// <summary>
	/// The amount of times the list was ordered.
	/// </summary>
	public Counter OrderCount { get; }

	/// <summary>
	/// The number of items removed the list.
	/// </summary>
	public Counter RemovedCount { get; }

	#endregion
}