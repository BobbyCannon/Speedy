#region References

using System;
using Speedy.Data;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationComparer : Comparer<Location>
{
	#region Constructors

	/// <summary>
	/// Instantiate a state comparer for the <see cref="Location" /> type.
	/// </summary>
	public LocationComparer()
	{
		AlwaysTrustSameSource = true;
		SourceTimeout = TimeSpan.FromSeconds(1);
	}

	#endregion

	#region Properties

	/// <summary>
	/// If true all updates from the existing source will always be accepted regardless of
	/// the quality of the data.
	/// </summary>
	public bool AlwaysTrustSameSource { get; set; }

	/// <summary>
	/// The timeout before the current data will expire and allow data that is lower quality.
	/// </summary>
	public TimeSpan SourceTimeout { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool ValidateUpdate(Location update)
	{
		return ShouldUpdateVerticalLocation(update)
			|| ShouldUpdateHorizontalLocation(update);
	}

	/// <inheritdoc />
	protected override bool TryUpdateCurrentState(Location update)
	{
		var updated = false;

		if (ShouldUpdateVerticalLocation(update))
		{
			CurrentState.Altitude = update.Altitude;
			CurrentState.AltitudeReference = update.AltitudeReference;
			CurrentState.VerticalAccuracy = update.VerticalAccuracy;
			CurrentState.VerticalAccuracyReference = update.VerticalAccuracyReference;
			CurrentState.VerticalSourceName = update.VerticalSourceName;
			CurrentState.VerticalStatusTime = update.VerticalStatusTime;
			updated = true;
		}

		if (ShouldUpdateHorizontalLocation(update))
		{
			CurrentState.Heading = update.Heading;
			CurrentState.HorizontalAccuracy = update.HorizontalAccuracy;
			CurrentState.HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			CurrentState.HorizontalSourceName = update.HorizontalSourceName;
			CurrentState.HorizontalStatusTime = update.HorizontalStatusTime;
			CurrentState.Latitude = update.Latitude;
			CurrentState.LocationFlags = update.LocationFlags;
			CurrentState.Longitude = update.Longitude;
			CurrentState.Speed = update.Speed;
			updated = true;
		}

		if (updated)
		{
			// General updates...
			CurrentState.ProviderName = update.ProviderName;
		}

		return updated;
	}

	private bool ShouldUpdateHorizontalLocation(IHorizontalLocation update)
	{
		if (update.HorizontalStatusTime < CurrentState.HorizontalStatusTime)
		{
			// This is an old update so reject it
			return false;
		}

		if (update.HasHorizontalAccuracy
			&& CurrentState.HasHorizontalAccuracy
			&& (update.HorizontalAccuracy <= CurrentState.HorizontalAccuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		if (update.HasHorizontalAccuracy && !CurrentState.HasHorizontalAccuracy)
		{
			// The update has accuracy but the current state does not, so take the update
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (AlwaysTrustSameSource && (CurrentState.HorizontalSourceName == update.HorizontalSourceName))
		{
			return true;
		}

		// Has the current state expired?
		var elapsed = update.HorizontalStatusTime - CurrentState.HorizontalStatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	private bool ShouldUpdateVerticalLocation(IVerticalLocation update)
	{
		if (update.VerticalStatusTime < CurrentState.VerticalStatusTime)
		{
			// This is an old update so reject it
			return false;
		}

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
			// The update has accuracy but the current state does not, so take the update
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (AlwaysTrustSameSource && (CurrentState.VerticalSourceName == update.VerticalSourceName))
		{
			return true;
		}

		// Has the current state expired?
		var elapsed = update.VerticalStatusTime - CurrentState.VerticalStatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	#endregion
}