#region References

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for Task
	/// </summary>
	public static class TaskExtensions
	{
		#region Methods

		/// <summary>
		/// Await the results of a task.
		/// </summary>
		/// <param name="task"> The task to run. </param>
		public static void AwaitResults(this Task task)
		{
			task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Await the results of a task.
		/// </summary>
		/// <typeparam name="T"> The type to return. </typeparam>
		/// <param name="task"> The task to run. </param>
		/// <returns> The result of the task. </returns>
		public static T AwaitResults<T>(this Task<T> task)
		{
			return task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// Await the results of a task.
		/// </summary>
		/// <typeparam name="T"> The type to return. </typeparam>
		/// <param name="task"> The task to run. </param>
		/// <param name="timeout"> The timeout if the task does not complete. </param>
		/// <returns> The result of the task. </returns>
		public static T AwaitResults<T>(this Task<T> task, TimeSpan timeout)
		{
			return task.TimeoutAfter(timeout).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Determine if a task has started and is completed.
		/// </summary>
		/// <param name="task"> The task to check. </param>
		/// <returns> True if the task is Cancelled, Faulted, or RanToCompletion otherwise false. </returns>
		public static bool IsCompleted(this Task task)
		{
			return task.Status == TaskStatus.Canceled
				|| task.Status == TaskStatus.Faulted
				|| task.Status == TaskStatus.RanToCompletion;
		}

		private static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			using var timeoutCancellationTokenSource = new CancellationTokenSource();
			var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
			if (completedTask != task)
			{
				throw new TimeoutException("The operation has timed out.");
			}

			timeoutCancellationTokenSource.Cancel();

			// Very important in order to propagate exceptions
			return await task;
		}

		#endregion
	}
}