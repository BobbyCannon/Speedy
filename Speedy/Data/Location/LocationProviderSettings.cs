#region References

using System;
using Speedy.Extensions;

#endregion

namespace Speedy.Data.Location;

/// <summary>
/// Settings for a location provider.
/// </summary>
public class LocationProviderSettings : Bindable, ILocationProviderSettings
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
		ResetToDefaults();
	}

	static LocationProviderSettings()
	{
		DefaultDesiredAccuracy = 3;
		DesiredAccuracyLowerLimit = 1;
		DesiredAccuracyUpperLimit = 100;

		DefaultMinimumDistance = 3;
		MinimumDistanceLowerLimit = 0;
		MinimumDistanceUpperLimit = 100;

		DefaultMinimumTime = TimeSpan.FromSeconds(1);
		MinimumTimeLowerLimit = TimeSpan.FromSeconds(1);
		MinimumTimeUpperLimit = TimeSpan.FromSeconds(100);

		DefaultRequireLocationAlwaysPermission = true;

		// iOS
		DefaultAllowBackgroundUpdates = true;
		DefaultAllowPausingOfUpdates = false;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool AllowBackgroundUpdates { get; set; }

	/// <inheritdoc />
	public bool AllowPausingOfUpdates { get; set; }

	/// <summary>
	/// Default for <see cref="AllowBackgroundUpdates" />.
	/// </summary>
	public static bool DefaultAllowBackgroundUpdates { get; set; }

	/// <summary>
	/// Default for <see cref="AllowPausingOfUpdates" />.
	/// </summary>
	public static bool DefaultAllowPausingOfUpdates { get; set; }

	/// <summary>
	/// Global default desired accuracy in meters.
	/// </summary>
	public static int DefaultDesiredAccuracy { get; set; }

	/// <summary>
	/// The default minimum distance to travel for updates.
	/// </summary>
	public static int DefaultMinimumDistance { get; set; }

	/// <summary>
	/// The default requested time period between updates.
	/// </summary>
	public static TimeSpan DefaultMinimumTime { get; set; }

	/// <summary>
	/// Gets or set flag to require always permission. If true always require otherwise "only in use" permission.
	/// </summary>
	public static bool DefaultRequireLocationAlwaysPermission { get; set; }

	/// <inheritdoc />
	public int DesiredAccuracy { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="DesiredAccuracy" />.
	/// </summary>
	public static int DesiredAccuracyLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="DesiredAccuracy" />.
	/// </summary>
	public static int DesiredAccuracyUpperLimit { get; set; }

	/// <inheritdoc />
	public int MinimumDistance { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="MinimumDistance" />.
	/// </summary>
	public static int MinimumDistanceLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="MinimumDistance" />.
	/// </summary>
	public static int MinimumDistanceUpperLimit { get; set; }

	/// <inheritdoc />
	public TimeSpan MinimumTime { get; set; }

	/// <summary>
	/// The lower range limit for <see cref="MinimumTime" />.
	/// </summary>
	public static TimeSpan MinimumTimeLowerLimit { get; set; }

	/// <summary>
	/// The upper range limit for <see cref="MinimumTime" />.
	/// </summary>
	public static TimeSpan MinimumTimeUpperLimit { get; set; }

	/// <inheritdoc />
	public bool RequireLocationAlwaysPermission { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual void Cleanup()
	{
		this.IfThen(x => x.DesiredAccuracy < DesiredAccuracyLowerLimit, x => x.DesiredAccuracy = DesiredAccuracyLowerLimit);
		this.IfThen(x => x.DesiredAccuracy > DesiredAccuracyUpperLimit, x => x.DesiredAccuracy = DesiredAccuracyUpperLimit);

		this.IfThen(x => x.MinimumDistance < MinimumDistanceLowerLimit, x => x.MinimumDistance = MinimumDistanceLowerLimit);
		this.IfThen(x => x.MinimumDistance > MinimumDistanceUpperLimit, x => x.MinimumDistance = MinimumDistanceUpperLimit);

		this.IfThen(x => x.MinimumTime < MinimumTimeLowerLimit, x => x.MinimumTime = MinimumTimeLowerLimit);
		this.IfThen(x => x.MinimumTime > MinimumTimeUpperLimit, x => x.MinimumTime = MinimumTimeUpperLimit);
	}

	/// <inheritdoc />
	public virtual void Reset()
	{
		ResetToDefaults();
	}

	private void ResetToDefaults()
	{
		AllowBackgroundUpdates = DefaultAllowBackgroundUpdates;
		AllowPausingOfUpdates = DefaultAllowPausingOfUpdates;
		DesiredAccuracy = DefaultDesiredAccuracy;
		MinimumDistance = DefaultMinimumDistance;
		MinimumTime = DefaultMinimumTime;
		RequireLocationAlwaysPermission = DefaultRequireLocationAlwaysPermission;
	}

	#endregion
}

/// <summary>
/// Represents settings for a location provider.
/// </summary>
public interface ILocationProviderSettings : IBindable
{
	#region Properties

	/// <summary>
	/// Determines whether or not location updates are allowed from a suspended app
	/// </summary>
	/// <remarks>
	/// Only supported in iOS at the moment. If true, the Location Background Mode must be set in the Info.plist.
	/// </remarks>
	bool AllowBackgroundUpdates { get; set; }

	/// <summary>
	/// Determines whether or not location updates are allow to pause.
	/// </summary>
	/// <remarks>
	/// Only supported in iOS at the moment.
	/// </remarks>
	bool AllowPausingOfUpdates { get; set; }

	/// <summary>
	/// Desired accuracy in meters
	/// </summary>
	int DesiredAccuracy { get; set; }

	/// <summary>
	/// The minimum distance to travel for updates.
	/// </summary>
	int MinimumDistance { get; set; }

	/// <summary>
	/// The requested time period between updates.
	/// </summary>
	TimeSpan MinimumTime { get; set; }

	/// <summary>
	/// Gets or set flag to require always permission. If true always require otherwise "only in use" permission.
	/// </summary>
	bool RequireLocationAlwaysPermission { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Cleanup the settings to be sure they are in range.
	/// </summary>
	void Cleanup();

	/// <summary>
	/// Reset the settings back to defaults.
	/// </summary>
	void Reset();

	#endregion
}