#region References

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using Timer = Speedy.Profiling.Timer;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// General utility extensions
	/// </summary>
	public static class UtilityExtensions
	{
		#region Methods

		/// <summary>
		/// Runs action if the test is true.
		/// </summary>
		/// <param name="item"> The item to process. (does nothing) </param>
		/// <param name="test"> The test to validate. </param>
		/// <param name="action"> The action to run if test is true. </param>
		/// <typeparam name="T"> The type the function returns </typeparam>
		public static void IfThen<T>(this T item, Func<T, bool> test, Action<T> action)
		{
			if (test(item))
			{
				action(item);
			}
		}

		/// <summary>
		/// Runs action1 if the test is true or action2 if false.
		/// </summary>
		/// <param name="item"> The item to process. (does nothing) </param>
		/// <param name="test"> The test to validate. </param>
		/// <param name="action1"> The action to run if test is true. </param>
		/// <param name="action2"> The action to run if test is false. </param>
		/// <typeparam name="T"> The type the function returns </typeparam>
		public static void IfThenElse<T>(this T item, Func<T, bool> test, Action<T> action1, Action<T> action2)
		{
			if (test(item))
			{
				action1(item);
			}
			else
			{
				action2(item);
			}
		}

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <typeparam name="T"> The type for this retry. </typeparam>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		public static T Retry<T>(Func<T> action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				return action();
			}
			catch (Exception)
			{
				Thread.Sleep(delay);

				var remaining = (int) (timeout - watch.Elapsed.TotalMilliseconds);
				if (remaining <= 0)
				{
					throw;
				}

				return Retry(action, remaining, delay);
			}
		}

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		public static void Retry(Action action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				action();
			}
			catch (Exception)
			{
				Thread.Sleep(delay);

				var remaining = (int) (timeout - watch.Elapsed.TotalMilliseconds);
				if (remaining <= 0)
				{
					throw;
				}

				Retry(action, remaining, delay);
			}
		}

		/// <summary>
		/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
		/// time.
		/// </summary>
		/// <param name="action"> The action to call. </param>
		/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
		/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
		/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		public static bool Wait(Func<bool> action, int timeout, int delay, bool useTimeService = false)
		{
			// Leave here for performance reason, in case cancellation has already been requested
			// Note: this cut 75% of time for existing cancellations
			if (action())
			{
				return true;
			}

			var watch = Stopwatch.StartNew();
			var timer = Timer.StartNew();
			var elapsed = () => useTimeService ? timer.Elapsed : watch.Elapsed;
			var watchTimeout = TimeSpan.FromMilliseconds(timeout);

			while (!action())
			{
				if (elapsed() > watchTimeout)
				{
					return false;
				}

				Thread.Sleep(delay);
			}

			return true;
		}

		/// <summary>
		/// Wait for a cancellation or for the value to time out.
		/// </summary>
		/// <param name="cancellationPending"> A check for cancellation. </param>
		/// <param name="value"> The value of time to wait for. </param>
		/// <param name="delay"> The delay between checks. </param>
		/// <param name="minimum"> The minimal time to wait. </param>
		/// <param name="maximum"> The maximum time to wait. </param>
		/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
		/// <returns> True if the wait was completed, false if the wait was cancelled. </returns>
		public static bool Wait(Func<bool> cancellationPending, TimeSpan value, TimeSpan delay, TimeSpan minimum, TimeSpan maximum, bool useTimeService = false)
		{
			// Leave here for performance reason, in case cancellation has already been requested
			// Note: this cut 75% of time for existing cancellations
			if (cancellationPending())
			{
				return false;
			}

			var watch = Stopwatch.StartNew();
			var timer = Timer.StartNew();
			var elapsed = () => useTimeService ? timer.Elapsed : watch.Elapsed;
			var shouldDelay = delay.Ticks > 0;

			while (((elapsed() < value) || (elapsed() < minimum)) && (elapsed() < maximum))
			{
				if (cancellationPending())
				{
					return false;
				}

				if (shouldDelay)
				{
					Thread.Sleep(delay);
				}
			}

			return true;
		}

		#endregion
	}
}