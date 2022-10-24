#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Settings for a location provider.
/// </summary>
public class LocationProviderSettings : Bindable
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the settings.
	/// </summary>
	public LocationProviderSettings() : this(null)
	{
	}

	/// <summary>
	/// Instantiates an instance of the settings.
	/// </summary>
	public LocationProviderSettings(IDispatcher dispatcher) : base(dispatcher)
	{
		DefaultTimeout = TimeSpan.FromSeconds(1);
		DesiredAccuracy = 10;
		LocationChangeTimeout = TimeSpan.FromSeconds(60);
		MinimumDistance = 10;
		MinimumTime = TimeSpan.FromSeconds(1);
	}

	static LocationProviderSettings()
	{
		DefaultTimeoutLowerLimit = TimeSpan.FromSeconds(1);
		DefaultTimeoutUpperLimit = TimeSpan.FromSeconds(100);

		DesiredAccuracyLowerLimit = 1;
		DesiredAccuracyUpperLimit = 100;

		MinimumDistanceLowerLimit = 1;
		MinimumDistanceUpperLimit = 100;

		MinimumTimeLowerLimit = TimeSpan.FromSeconds(1);
		MinimumTimeUpperLimit = TimeSpan.FromSeconds(100);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Default timeout to be used when timeout is not provided.
	/// </summary>
	public TimeSpan DefaultTimeout { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="DefaultTimeout" />.
	/// </summary>
	public static TimeSpan DefaultTimeoutLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="DefaultTimeout" />.
	/// </summary>
	public static TimeSpan DefaultTimeoutUpperLimit { get; set; }

	/// <summary>
	/// Desired accuracy in meters
	/// </summary>
	public double DesiredAccuracy { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="DesiredAccuracy" />.
	/// </summary>
	public static double DesiredAccuracyLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="DesiredAccuracy" />.
	/// </summary>
	public static double DesiredAccuracyUpperLimit { get; set; }

	/// <summary>
	/// The threshold for accuracy changes to allow provider to switch.
	/// </summary>
	public double LocationChangeAccuracyThreshold { get; set; }

	/// <summary>
	/// The timeout in which the last read location will expire.
	/// </summary>
	public TimeSpan LocationChangeTimeout { get; set; }

	/// <summary>
	/// The minimum distance to travel for updates.
	/// </summary>
	public double MinimumDistance { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="MinimumDistance" />.
	/// </summary>
	public static double MinimumDistanceLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="MinimumDistance" />.
	/// </summary>
	public static double MinimumDistanceUpperLimit { get; set; }

	/// <summary>
	/// The requested time period between updates.
	/// </summary>
	public TimeSpan MinimumTime { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="MinimumTime" />.
	/// </summary>
	public static TimeSpan MinimumTimeLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="MinimumTime" />.
	/// </summary>
	public static TimeSpan MinimumTimeUpperLimit { get; set; }

	/// <summary>
	/// Gets or set flag to require always permission. If true always require otherwise "only in use" permission.
	/// </summary>
	public bool RequireLocationAlwaysPermission { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Cleanup the settings to be sure they are in range.
	/// </summary>
	protected internal virtual void Cleanup()
	{
		this.IfThen(x => x.DefaultTimeout < DefaultTimeoutLowerLimit, x => x.DefaultTimeout = DefaultTimeoutLowerLimit);
		this.IfThen(x => x.DefaultTimeout > DefaultTimeoutUpperLimit, x => x.DefaultTimeout = DefaultTimeoutUpperLimit);

		this.IfThen(x => x.DesiredAccuracy < DesiredAccuracyLowerLimit, x => x.DesiredAccuracy = DesiredAccuracyLowerLimit);
		this.IfThen(x => x.DesiredAccuracy > DesiredAccuracyUpperLimit, x => x.DesiredAccuracy = DesiredAccuracyUpperLimit);

		this.IfThen(x => x.MinimumDistance < MinimumDistanceLowerLimit, x => x.MinimumDistance = MinimumDistanceLowerLimit);
		this.IfThen(x => x.MinimumDistance > MinimumDistanceUpperLimit, x => x.MinimumDistance = MinimumDistanceUpperLimit);

		this.IfThen(x => x.MinimumTime < MinimumTimeLowerLimit, x => x.MinimumTime = MinimumTimeLowerLimit);
		this.IfThen(x => x.MinimumTime > MinimumTimeUpperLimit, x => x.MinimumTime = MinimumTimeUpperLimit);
	}

	#endregion
}