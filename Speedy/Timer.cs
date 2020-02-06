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
	public class Timer
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
		public Timer()
		{
			Reset();
		}

		/// <summary>
		/// Static constructor for stopwatch.
		/// </summary>
		static Timer()
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
		/// Initializes a new instance of a timer from a specific time.
		/// </summary>
		/// <returns> The started timer from provided DateTime. </returns>
		public static Timer CreateNew(DateTime start)
		{
			var response = StartNew(start);
			response.Stop();
			return response;
		}

		/// <summary>
		/// Gets the current counter. For High Resolution timers this will be the performance
		/// counter otherwise it will be the ticks for the UTC DateTime.
		/// </summary>
		/// <returns> Returns the current counter. </returns>
		public static long GetCurrentCounter(bool highResolution)
		{
			if (!IsHighResolutionAvailable || !highResolution)
			{
				return TimeService.UtcNow.Ticks;
			}

			QueryPerformanceCounter(out var timestamp);
			return timestamp;
		}

		/// <summary>
		/// Stops the timer and resets the elapsed time.
		/// </summary>
		public void Reset()
		{
			_elapsed = 0;
			_start = 0;
			IsRunning = false;
			IsHighResolution = true;
		}

		/// <summary>
		/// Stops the timer, resets the elapsed time, then restarts timer.
		/// </summary>
		public void Restart()
		{
			_elapsed = 0;
			_start = GetCurrentCounter(true);
			IsRunning = true;
			IsHighResolution = true;
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

			IsHighResolution = true;
			_start = GetCurrentCounter(IsHighResolution);
			IsRunning = true;
		}

		/// <summary>
		/// Initializes a new instance of a timer and starts it.
		/// </summary>
		/// <returns> The started timer. </returns>
		public static Timer StartNew()
		{
			var response = new Timer();
			response.Start();
			return response;
		}

		/// <summary>
		/// Initializes a new instance of a timer from a specific time.
		/// </summary>
		/// <returns> The started timer from provided DateTime. </returns>
		public static Timer StartNew(DateTime start)
		{
			var response = new Timer
			{
				_start = start.Ticks,
				IsHighResolution = false,
				IsRunning = true
			};

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
			IsRunning = false;

			if (_elapsed < 0)
			{
				// Never allows negative elapsed, can happen on some hardware.
				_elapsed = 0;
			}
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

		#endregion
	}
}