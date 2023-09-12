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
		OrderCount = new Counter(dispatcher);
	}

	#endregion

	#region Properties

	/// <summary>
	/// The amount of times the list was ordered.
	/// </summary>
	public Counter OrderCount { get; }

	#endregion
}