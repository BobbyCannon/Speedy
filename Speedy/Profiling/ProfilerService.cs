#region References

using System;
using System.Diagnostics;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// Class for profiling actions
/// </summary>
public abstract class ProfilerService : Bindable
{
	#region Constructors

	/// <summary>
	/// Instantiate an instance of the provider service.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected ProfilerService(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Profile an action.
	/// </summary>
	/// <param name="action"> The action to profile. </param>
	/// <returns> The time the action took. </returns>
	public TimeSpan Profile(Action action)
	{
		var watch = Stopwatch.StartNew();
		action();
		watch.Stop();
		return watch.Elapsed;
	}

	#endregion
}