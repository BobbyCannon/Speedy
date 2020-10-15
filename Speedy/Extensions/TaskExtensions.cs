using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for Task
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Determine if a task has started and is completed.
		/// </summary>
		/// <param name="task"> The task to check. </param>
		/// <returns> True if the task is Cancelled, Faulted, or RanToCompletion otherwise false.</returns>
		public static bool IsCompleted(this Task task) =>
			task.Status == TaskStatus.Canceled
			|| task.Status == TaskStatus.Faulted
			|| task.Status == TaskStatus.RanToCompletion;

	}
}
