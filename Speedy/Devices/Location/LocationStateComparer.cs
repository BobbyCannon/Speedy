#region References

using System;
using Speedy.Data;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationStateComparer : StateComparer<Location>
{
	#region Properties

	public bool AlwaysTrustSameSource { get; set; }

	public TimeSpan SourceTimeout { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool ShouldUpdateState(Location update)
	{
		return IsUpdateDesired(update)
			|| HasBetterLocation(update);
	}

	/// <inheritdoc />
	protected override bool TryUpdateCurrentState(Location update)
	{
		var updated = false;

		if (IsUpdateDesired(update))
		{
			CurrentState.Altitude = update.Altitude;
			CurrentState.AltitudeReference = update.AltitudeReference;
			CurrentState.VerticalAccuracy = update.VerticalAccuracy;
			CurrentState.VerticalAccuracyReference = update.VerticalAccuracyReference;
			CurrentState.VerticalSourceName = update.VerticalSourceName;
			CurrentState.VerticalStatusTime = update.VerticalStatusTime;
			updated = true;
		}

		return updated;
	}

	private bool HasBetterLocation(IHorizontalLocation update)
	{
		if (update.HasHorizontalAccuracy
			&& CurrentState.HasHorizontalAccuracy
			&& (update.HorizontalAccuracy <= CurrentState.HorizontalAccuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		return false;
	}

	private bool IsUpdateDesired(IVerticalLocation update)
	{
		if (update.HasVerticalAccuracy
			&& update.HasAltitude
			&& CurrentState.HasVerticalAccuracy
			&& CurrentState.HasAltitude
			&& (update.VerticalAccuracy <= CurrentState.VerticalAccuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		// todo: should we have an accuracy limit? or does "better" accurate update handle

		if (update.HasVerticalAccuracy && !CurrentState.HasVerticalAccuracy)
		{
			// Both have altitude accuracy and the update is better
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (CurrentState.VerticalSourceName == update.VerticalSourceName)
		{
			return true;
		}

		// Has the cu
		var now = TimeService.UtcNow;
		var elapsed = now - CurrentState.VerticalStatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	#endregion
}