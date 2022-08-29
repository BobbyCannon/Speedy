#region References

using System;
using System.Diagnostics;

#endregion

namespace Speedy.Profiling
{
	/// <summary>
	/// Class for profiling actions
	/// </summary>
	public static class Profiler
	{
		#region Methods

		/// <summary>
		/// Profile an action.
		/// </summary>
		/// <param name="action"> The action to profile. </param>
		/// <returns> The time the action took. </returns>
		public static TimeSpan Profile(this Action action)
		{
			var watch = Stopwatch.StartNew();
			action();
			watch.Stop();
			return watch.Elapsed;
		}

		#endregion
	}
}