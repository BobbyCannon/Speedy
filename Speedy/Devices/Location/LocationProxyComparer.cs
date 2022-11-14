#region References

using System;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationProxyComparer<TLocation> : Comparer<TLocation>
	where TLocation : class, ILocationProxy, new()
{
	#region Constructors

	/// <summary>
	/// Instantiate a state comparer for the <see cref="Location" /> type.
	/// </summary>
	public LocationProxyComparer()
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
		return ShouldUpdateLocation(Value, update);
	}

	/// <inheritdoc />
	protected override bool TryUpdateValue(TLocation update)
	{
		var updated = false;

		if (ShouldUpdateLocation(Value, update))
		{
			Value.UpdateValues(update);
			//Value.Altitude = update.Altitude;
			//Value.Reference = update.AltitudeReference;
			//Value.VerticalAccuracy = update.VerticalAccuracy;
			//Value.VerticalAccuracyReference = update.VerticalAccuracyReference;
			//Value.VerticalFlags = update.VerticalFlags;
			//Value.VerticalSourceName = update.VerticalSourceName;
			//Value.VerticalStatusTime = update.VerticalStatusTime;
			updated = true;
		}
		
		return updated;
	}

	private bool ShouldUpdateLocation(ILocationProxy current, ILocationProxy update)
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

	#endregion
}