#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Results for the profiler.
	/// </summary>
	public class ProfilerSession
	{
		#region Constructors

		/// <summary>
		/// Instantiate a profile result.
		/// </summary>
		/// <param name="name"> The name of the profile session result. </param>
		public ProfilerSession(string name)
		{
			Name = name;
			Sessions = new List<ProfilerSession>();
			StartedOn = TimeService.UtcNow;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the profile session.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Child sessions for this session.
		/// </summary>
		public IList<ProfilerSession> Sessions { get; }

		/// <summary>
		/// The date time the session started.
		/// </summary>
		public DateTime StartedOn { get; set; }

		/// <summary>
		/// The date time the session stopped.
		/// </summary>
		public DateTime StoppedOn { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Stop this profiler session.
		/// </summary>
		public virtual void Stop()
		{
			StoppedOn = TimeService.UtcNow;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Name}: {StoppedOn - StartedOn}";
		}

		/// <summary>
		/// Trigger the session to store a result.
		/// </summary>
		/// <param name="name"> The name of the session section. </param>
		public virtual void Trigger(Func<string> name)
		{
		}

		#endregion
	}
}