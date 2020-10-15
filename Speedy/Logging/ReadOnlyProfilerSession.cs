#region References

using System;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Read only results for the profiler.
	/// </summary>
	public class ReadOnlyProfilerSession : ProfilerSession
	{
		#region Constructors

		/// <summary>
		/// Instantiate a read only profile result.
		/// </summary>
		/// <param name="name"> The name of the profile session result. </param>
		public ReadOnlyProfilerSession(string name) : base(name)
		{
			StartedOn = DateTime.MinValue;
			StoppedOn = DateTime.MinValue;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override void Stop()
		{
			// N/A
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Name}: read only session";
		}

		/// <inheritdoc />
		public override void Trigger(Func<string> name)
		{
			// N/A
		}

		#endregion
	}
}