#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Low overhead profiler.
	/// </summary>
	public static class Profiler
	{
		#region Fields

		private static readonly ReadOnlyProfilerSession _readOnlySession;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the profiler.
		/// </summary>
		static Profiler()
		{
			_readOnlySession = new ReadOnlyProfilerSession("Disabled");

			Results = new List<ProfilerSession>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Flag to determine if the profiler is enabled.
		/// </summary>
		public static bool Enabled { get; set; }

		/// <summary>
		/// Results for the profiler.
		/// </summary>
		public static IList<ProfilerSession> Results { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Reset the profiler.
		/// </summary>
		public static void Reset()
		{
			Enabled = false;
			Results.Clear();
		}

		/// <summary>
		/// Start a profile session.
		/// </summary>
		/// <param name="getName"> </param>
		/// <returns> </returns>
		public static ProfilerSession Start(Func<string> getName)
		{
			if (!Enabled)
			{
				return _readOnlySession;
			}

			var name = getName();
			var result = new ProfilerSession(name);
			Results.Add(result);
			return result;
		}

		#endregion
	}
}