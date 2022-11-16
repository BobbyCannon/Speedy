#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="Location" /> type.
/// </summary>
public class LocationComparer<T> : Comparer<T>
	where T : ILocationDeviceInformation, IUpdatable<T>
{
	#region Constructors

	/// <summary>
	/// Instantiate a state comparer for the <see cref="Location" /> type.
	/// </summary>
	public LocationComparer() : this(null)
	{
	}

	/// <summary>
	/// Instantiate a state comparer for the <see cref="Location" /> type.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public LocationComparer(IDispatcher dispatcher) : base(dispatcher)
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
	public override bool ShouldUpdate(T current, T update)
	{
		if (update.StatusTime < current.StatusTime)
		{
			// This is an old update so reject it
			return false;
		}

		if (update.HasAccuracy
			&& update.HasValue
			&& current.HasAccuracy
			&& current.HasValue
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

	/// <inheritdoc />
	public override bool UpdateWith(ref T value, T update, params string[] exclusions)
	{
		return value.UpdateWith(update, exclusions);
	}

	#endregion
}