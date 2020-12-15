#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Profiling
{
	/// <summary>
	/// In memory tracker path repository.
	/// </summary>
	public class MemoryTrackerPathRepository : ITrackerPathRepository
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the path repository.
		/// </summary>
		public MemoryTrackerPathRepository()
		{
			Paths = new List<TrackerPath>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The paths for this repository.
		/// </summary>
		public List<TrackerPath> Paths { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Clears the paths from the repository.
		/// </summary>
		public void Clear()
		{
			Paths.Clear();
		}

		/// <summary>
		/// Writes paths to this repository.
		/// </summary>
		/// <param name="paths"> The paths to be added. </param>
		public void Write(params TrackerPath[] paths)
		{
			Paths.AddRange(paths);
		}

		#endregion
	}
}