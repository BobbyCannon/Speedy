#region References

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#endregion

namespace Speedy
{
	/// <summary>
	/// Provides a set of methods and properties that you can use to accurately measure elapsed time.
	/// </summary>
	public class Stopwatch
	{
		#region Constants

		/// <summary>
		/// Ticks per millisecond.
		/// </summary>
		public const long TicksPerMillisecond = 10000;

		/// <summary>
		/// Ticks per second.
		/// </summary>
		public const long TicksPerSecond = TicksPerMillisecond * 1000;

		#endregion

		#region Fields

		private long _elapsed;
		private long _start;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of a stopwatch.
		/// </summary>
		public Stopwatch()
		{
			Reset(0, IsHighResolutionAvailable);
		}

		/// <summary>
		/// Instantiates an instance of a stopwatch.
		/// </summary>
		public Stopwatch(DateTime startTime)
		{
			Reset(startTime.Ticks, false);
		}

		/// <summary>
		/// Static constructor for stopwatch.
		/// </summary>
		static Stopwatch()
		{
			if (QueryPerformanceFrequency(out var frequency))
			{
				IsHighResolutionAvailable = true;
				Frequency = frequency;
				TickFrequency = TicksPerSecond;
				TickFrequency /= Frequency;
			}
			else
			{
				IsHighResolutionAvailable = false;
				Frequency = TicksPerSecond;
				TickFrequency = 1;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the total elapsed time measured by the current instance.
		/// </summary>
		/// <returns> A read-only <see cref="T:System.TimeSpan"> </see> representing the total elapsed time measured by the current instance. </returns>
		public TimeSpan Elapsed => new TimeSpan(GetElapsedTicks(_elapsed, _start, IsHighResolution, IsRunning));

		/// <summary>
		/// Retrieves the frequency of the performance counter.
		/// </summary>
		public static long Frequency { get; }

		/// <summary>
		/// Gets a value indicating if the timer is high resolution.
		/// </summary>
		public bool IsHighResolution { get; private set; }

		/// <summary>
		/// Gets a value indicating if high resolution timers are available.
		/// </summary>
		public static bool IsHighResolutionAvailable { get; }

		/// <summary>
		/// Gets a value indicating if the timer is running.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Retrieves the counter frequency counts per ticks.
		/// </summary>
		public static double TickFrequency { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Stops the timer and resets the elapsed time.
		/// </summary>
		public void Reset()
		{
			Reset(0, IsHighResolutionAvailable);
		}

		/// <summary>
		/// Stops the timer, resets the elapsed time, then restarts timer.
		/// </summary>
		public void Restart()
		{
			_elapsed = 0;
			IsHighResolution = IsHighResolutionAvailable;
			_start = GetCurrentCounter(IsHighResolution);
			IsRunning = true;
		}

		/// <summary>
		/// Stops the timer, resets the elapsed time, then restarts timer.
		/// </summary>
		/// <param name="start"> The time the timer should start from. </param>
		public void Restart(DateTime start)
		{
			_elapsed = 0;
			_start = start.Ticks;
			IsHighResolution = false;
			IsRunning = true;
		}

		/// <summary>
		/// Starts or resumes the timer.
		/// </summary>
		public void Start()
		{
			if (IsRunning)
			{
				return;
			}

			IsHighResolution = IsHighResolutionAvailable;
			_start = GetCurrentCounter(IsHighResolution);
			IsRunning = true;
		}

		/// <summary>
		/// Initializes a new instance of a timer and starts it.
		/// </summary>
		/// <returns> The started timer. </returns>
		public static Stopwatch StartNew()
		{
			var response = new Stopwatch();
			response.Start();
			return response;
		}

		/// <summary>
		/// Initializes a new instance of a timer from a specific time.
		/// </summary>
		/// <param name="startTime"> The time the timer should start from. </param>
		/// <returns> The started timer from provided DateTime. </returns>
		public static Stopwatch StartNew(DateTime startTime)
		{
			var response = new Stopwatch(startTime);
			response.Start();
			return response;
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			if (!IsRunning)
			{
				return;
			}

			var endTimeStamp = GetCurrentCounter(IsHighResolution);
			var elapsedThisPeriod = endTimeStamp - _start;

			_elapsed += elapsedThisPeriod;

			if (_elapsed < 0)
			{
				// Never allows negative elapsed, can happen on some hardware.
				_elapsed = 0;
			}

			IsHighResolution = IsHighResolutionAvailable;
			IsRunning = false;
		}

		/// <summary>
		/// Gets the current counter. For High Resolution timers this will be the performance
		/// counter otherwise it will be the ticks for the UTC DateTime.
		/// </summary>
		/// <returns> Returns the current counter. </returns>
		private static long GetCurrentCounter(bool highResolution)
		{
			if (!IsHighResolutionAvailable || !highResolution)
			{
				return TimeService.UtcNow.Ticks;
			}

			QueryPerformanceCounter(out var timestamp);
			return timestamp;
		}

		private static long GetElapsedCounter(long elapsed, long start, bool highResolution, bool isRunning)
		{
			var timeElapsed = elapsed;

			if (!isRunning)
			{
				return timeElapsed;
			}

			// If the StopWatch is running, add elapsed time since the Stopwatch is started last time. 
			var currentTicks = GetCurrentCounter(highResolution);
			var difference = currentTicks - start;
			timeElapsed += difference;

			return timeElapsed;
		}

		private static long GetElapsedTicks(long elapsed, long start, bool highResolution, bool isRunning)
		{
			var ticks = GetElapsedCounter(elapsed, start, highResolution, isRunning);

			if (!highResolution)
			{
				return ticks;
			}

			// Convert high resolution counter to DateTime ticks
			double lowResolutionTicks = ticks;
			lowResolutionTicks *= TickFrequency;
			return unchecked((long) lowResolutionTicks);
		}

		[DllImport("kernel32.dll")]
		[ResourceExposure(ResourceScope.None)]
		private static extern bool QueryPerformanceCounter(out long value);

		[DllImport("kernel32.dll")]
		[ResourceExposure(ResourceScope.None)]
		private static extern bool QueryPerformanceFrequency(out long value);

		/// <summary>
		/// Stops the timer and resets the elapsed time.
		/// </summary>
		private void Reset(long startTime, bool highResolution)
		{
			_elapsed = 0;
			_start = startTime;
			IsHighResolution = IsHighResolutionAvailable & highResolution;

			if (_start > 0)
			{
				IsRunning = true;
				Stop();
			}
			else
			{
				IsRunning = false;
			}
		}

		#endregion
	}
}