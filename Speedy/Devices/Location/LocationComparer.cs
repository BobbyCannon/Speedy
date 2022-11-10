#region References

using System;
using Speedy.Data;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationComparer : LocationComparer<Location, IHorizontalLocation, IVerticalLocation>
{
	#region Methods

	/// <inheritdoc />
	protected override bool TryUpdateValue(Location update)
	{
		var response = base.TryUpdateValue(update);

		if (response)
		{
			// General updates...
			Value.ProviderName = update.ProviderName;
		}

		return response;
	}

	#endregion
}

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationComparer<TLocation, THorizontal, TVertical> : Comparer<TLocation>
	where TLocation : class, THorizontal, TVertical, new()
	where THorizontal : IHorizontalLocation
	where TVertical : IVerticalLocation
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
	public override bool ValidateUpdate(TLocation update)
	{
		return ShouldUpdateVerticalLocation(update)
			|| ShouldUpdateHorizontalLocation(update);
	}

	/// <inheritdoc />
	protected override bool TryUpdateValue(TLocation update)
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

		return updated;
	}

	private bool ShouldUpdateHorizontalLocation(IHorizontalLocation update)
	{
		var current = new LocationProxy((IHorizontalLocation) Value);
		var updateProxy = new LocationProxy(update);
		return ShouldUpdateLocation(current, updateProxy);
	}

	private bool ShouldUpdateLocation(LocationProxy current, LocationProxy update)
	{
		if (update.StatusTime < current.StatusTime)
		{
			// This is an old update so reject it
			return false;
		}

		if (update.HasAccuracy
			&& update.HasLocation
			&& current.HasAccuracy
			&& current.HasLocation
			&& (update.Accuracy <= current.Accuracy))
		{
			// Both have altitude and accuracy and the update is better
			return true;
		}

		// todo: should we have an accuracy limit? or does "better" accurate update handle

		if (update.HasAccuracy && !current.HasAccuracy)
		{
			// The update has accuracy but the current state does not, so take the update
			return true;
		}

		// You may have an update from the same source but it's not as accurate
		if (AlwaysTrustSameSource && (current.SourceName == update.SourceName))
		{
			return true;
		}

		// Has the current state expired?
		var elapsed = update.StatusTime - current.StatusTime;
		if (elapsed >= SourceTimeout)
		{
			return true;
		}

		return false;
	}

	private bool ShouldUpdateVerticalLocation(IVerticalLocation update)
	{
		var current = new LocationProxy((IVerticalLocation) Value);
		var updateProxy = new LocationProxy(update);
		return ShouldUpdateLocation(current, updateProxy);
	}

	#endregion
}