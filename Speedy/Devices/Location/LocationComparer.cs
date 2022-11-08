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
		SourceTimeout = TimeSpan.FromSeconds(10);
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
	protected override bool TryUpdateValue(Location update)
	{
		var updated = false;

		if (ShouldUpdateVerticalLocation(update))
		{
			Value.Altitude = update.Altitude;
			Value.AltitudeReference = update.AltitudeReference;
			Value.VerticalAccuracy = update.VerticalAccuracy;
			Value.VerticalAccuracyReference = update.VerticalAccuracyReference;
			Value.VerticalFlags = update.VerticalFlags;
			Value.VerticalSourceName = update.VerticalSourceName;
			Value.VerticalStatusTime = update.VerticalStatusTime;
			updated = true;
		}

		if (ShouldUpdateHorizontalLocation(update))
		{
			Value.HorizontalHeading = update.HorizontalHeading;
			Value.HorizontalAccuracy = update.HorizontalAccuracy;
			Value.HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			Value.HorizontalFlags = update.HorizontalFlags;
			Value.HorizontalSourceName = update.HorizontalSourceName;
			Value.HorizontalStatusTime = update.HorizontalStatusTime;
			Value.Latitude = update.Latitude;
			Value.Longitude = update.Longitude;
			Value.HorizontalSpeed = update.HorizontalSpeed;
			updated = true;
		}

		if (updated)
		{
			// General updates...
			Value.ProviderName = update.ProviderName;
		}

		return updated;
	}

	private bool ShouldUpdateHorizontalLocation(IHorizontalLocation update)
	{
		if (update.HorizontalStatusTime < Value.HorizontalStatusTime)
		{
			// This is an old update so reject it
			return false;
		}

		if (update.HasHorizontalAccuracy
			&& Value.HasHorizontalAccuracy
			&& (update.HorizontalAccuracy <= Value.HorizontalAccuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		if (update.HasHorizontalAccuracy && !Value.HasHorizontalAccuracy)
		{
			// The update has accuracy but the current state does not, so take the update
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (AlwaysTrustSameSource && (Value.HorizontalSourceName == update.HorizontalSourceName))
		{
			return true;
		}

		// Has the current state expired?
		var elapsed = update.HorizontalStatusTime - Value.HorizontalStatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	private bool ShouldUpdateVerticalLocation(IVerticalLocation update)
	{
		if (update.VerticalStatusTime < Value.VerticalStatusTime)
		{
			// This is an old update so reject it
			return false;
		}

		if (update.HasVerticalAccuracy
			&& update.HasAltitude
			&& Value.HasVerticalAccuracy
			&& Value.HasAltitude
			&& (update.VerticalAccuracy <= Value.VerticalAccuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		// todo: should we have an accuracy limit? or does "better" accurate update handle

		if (update.HasVerticalAccuracy && !Value.HasVerticalAccuracy)
		{
			// The update has accuracy but the current state does not, so take the update
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (AlwaysTrustSameSource && (Value.VerticalSourceName == update.VerticalSourceName))
		{
			return true;
		}

		// Has the current state expired?
		var elapsed = update.VerticalStatusTime - Value.VerticalStatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	#endregion
}