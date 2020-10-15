#region References

using System;
using System.Diagnostics;
using System.Threading;

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
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		public static bool Wait(Func<bool> action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();
			var watchTimeout = TimeSpan.FromMilliseconds(timeout);
			var result = false;

			while (!result)
			{
				if (watch.Elapsed > watchTimeout)
				{
					return false;
				}

				result = action();

				if (!result)
				{
					Thread.Sleep(delay);
				}
			}

			return true;
		}

		#endregion
	}
}