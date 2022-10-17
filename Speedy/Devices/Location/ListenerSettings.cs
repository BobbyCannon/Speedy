using System;

namespace Speedy.Devices.Location;

/// <summary>
/// Settings for location listening.
/// </summary>
public class ListenerSettings : Bindable
{
	#region Constructors

	public ListenerSettings()
	{
	}

	public ListenerSettings(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	public bool IncludeHeading { get; set; }

	public double MinimumDistance { get; set; }

	public TimeSpan MinimumTime { get; set; }

	public bool RequireLocationAlwaysPermission { get; set; }

	#endregion

	#region Methods

	internal void Cleanup()
	{
		if (MinimumTime.TotalMilliseconds <= 0)
		{
			MinimumTime = TimeSpan.FromMilliseconds(10);
		}
		if (MinimumTime.TotalMilliseconds <= 0)
		{
			MinimumDistance = 3;
		}
	}

	#endregion
}