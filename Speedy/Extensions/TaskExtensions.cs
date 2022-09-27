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
		/// Synchronously await the results of an asynchronous operation without deadlocking.
		/// </summary>
		/// <param name="task"> The <see cref="Task" /> representing the pending operation. </param>
		public static void AwaitResults(this Task task)
		{
			SynchronousAwaiter.GetResult(task);
		}

		/// <summary>
		/// Synchronously await the results of an asynchronous operation without deadlocking.
		/// </summary>
		/// <typeparam name="T"> The result type of the operation. </typeparam>
		/// <param name="task"> The <see cref="Task" /> representing the pending operation. </param>
		/// <returns> The result of the operation. </returns>
		public static T AwaitResults<T>(this Task<T> task)
		{
			return SynchronousAwaiter.GetResult(task);
		}

		/// <summary>
		/// Synchronously await the results of an asynchronous operation without deadlocking.
		/// </summary>
		/// <typeparam name="T"> The result type of the operation. </typeparam>
		/// <param name="task"> The <see cref="Task" /> representing the pending operation. </param>
		/// <param name="timeout"> The timeout if the task does not complete. </param>
		/// <returns> The result of the operation. </returns>
		public static T AwaitResults<T>(this Task<T> task, TimeSpan timeout)
		{
			return SynchronousAwaiter.GetResult(task, timeout);
		}

		/// <summary>
		/// Determine if a task has started and is completed.
		/// </summary>
		/// <param name="task"> The task to check. </param>
		/// <returns> True if the task is Cancelled, Faulted, or RanToCompletion otherwise false. </returns>
		public static bool IsCompleted(this Task task)
		{
			return (task.Status == TaskStatus.Canceled)
				|| (task.Status == TaskStatus.Faulted)
				|| (task.Status == TaskStatus.RanToCompletion);
		}

		/// <summary>
		/// Timeout after some amount time.
		/// </summary>
		/// <typeparam name="TResult"> The type for the result. </typeparam>
		/// <param name="task"> The task to wait for. </param>
		/// <param name="timeout"> The maximum about of time to wait for. </param>
		/// <returns> The task with the result after waiting. </returns>
		/// <exception cref="TimeoutException"> </exception>
		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
		{
			using var timeoutCancellationTokenSource = new CancellationTokenSource();
			using var delay = Task.Delay(timeout, timeoutCancellationTokenSource.Token);

			var completedTask = await Task.WhenAny(task, delay).ConfigureAwait(false);
			if (completedTask != task)
			{
				throw new TimeoutException("The operation has timed out.");
			}

			timeoutCancellationTokenSource.Cancel();

			// Very important in order to propagate exceptions
			return await task;
		}

		#endregion

		#region Classes

		private class SynchronousAwaiter
		{
			#region Fields

			private Exception _exception;
			private object _result;

			#endregion

			#region Methods

			public static void GetResult(Task task)
			{
				var t = new SynchronousAwaiter();
				t.InternalGetResult(task);
			}

			public static T GetResult<T>(Task<T> task)
			{
				var t = new SynchronousAwaiter();
				return t.InternalGetResult(task);
			}

			public static T GetResult<T>(Task<T> task, TimeSpan timeout)
			{
				var t = new SynchronousAwaiter();
				return t.InternalGetResult(task, timeout);
			}

			private T InternalGetResult<T>(Task<T> task, TimeSpan timeout)
			{
				var manualResetEvent = new ManualResetEvent(false);
				WaitFor(task, manualResetEvent);
				var completed = manualResetEvent.WaitOne(timeout);

				if (_exception != null)
				{
					throw _exception;
				}

				if (!completed)
				{
					throw new TimeoutException("The operation has timed out.");
				}

				return (T) _result;
			}

			private void InternalGetResult(Task task)
			{
				var manualResetEvent = new ManualResetEvent(false);
				WaitFor(task, manualResetEvent);
				manualResetEvent.WaitOne();
				if (_exception != null)
				{
					throw _exception;
				}
			}

			private T InternalGetResult<T>(Task<T> task)
			{
				var manualResetEvent = new ManualResetEvent(false);
				WaitFor(task, manualResetEvent);
				manualResetEvent.WaitOne();
				if (_exception != null)
				{
					throw _exception;
				}

				return (T) _result;
			}

			private async void WaitFor<T>(Task<T> task, ManualResetEvent manualResetEvent)
			{
				try
				{
					_result = await task.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_exception = ex;
				}
				finally
				{
					manualResetEvent.Set();
				}
			}

			private async void WaitFor(Task task, ManualResetEvent manualResetEvent)
			{
				try
				{
					await task.ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
				}
				catch (Exception ex)
				{
					_exception = ex;
				}
				finally
				{
					manualResetEvent.Set();
				}
			}

			#endregion
		}

		#endregion
	}
}