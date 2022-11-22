#region References

using System;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The state comparer for the <see cref="ILocationDeviceInformation" /> type.
/// </summary>
public class LocationComparer<T, THorizontalLocation, TVerticalLocation>
	: Comparer<T>, IComparer<T, THorizontalLocation, TVerticalLocation>
	where T : class, ILocation<THorizontalLocation, TVerticalLocation>
	where THorizontalLocation : class, IHorizontalLocation, IUpdatable<THorizontalLocation>
	where TVerticalLocation : class, IVerticalLocation, IUpdatable<TVerticalLocation>
{
	#region Fields

	private readonly LocationDeviceInformationComparer<THorizontalLocation> _horizontalComparer;
	private readonly LocationDeviceInformationComparer<TVerticalLocation> _verticalComparer;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiate a state comparer.
	/// </summary>
	public LocationComparer() : this(null)
	{
	}

	/// <summary>
	/// Instantiate a state comparer.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public LocationComparer(IDispatcher dispatcher) : base(dispatcher)
	{
		AlwaysTrustSameSource = true;
		SourceTimeout = TimeSpan.FromSeconds(10);

		_horizontalComparer = new LocationDeviceInformationComparer<THorizontalLocation>(dispatcher)
		{
			AlwaysTrustSameSource = AlwaysTrustSameSource,
			SourceTimeout = SourceTimeout
		};

		_verticalComparer = new LocationDeviceInformationComparer<TVerticalLocation>(dispatcher)
		{
			AlwaysTrustSameSource = AlwaysTrustSameSource,
			SourceTimeout = SourceTimeout
		};
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
	public override bool ShouldUpdate(T value, T update)
	{
		return ShouldUpdate(value, update.HorizontalLocation)
			|| ShouldUpdate(value, update.VerticalLocation);
	}

	/// <inheritdoc />
	public bool ShouldUpdate(T value, THorizontalLocation update)
	{
		return _horizontalComparer.ShouldUpdate(value.HorizontalLocation, update);
	}

	/// <inheritdoc />
	public bool ShouldUpdate(T value, TVerticalLocation update)
	{
		return _verticalComparer.ShouldUpdate(value.VerticalLocation, update);
	}

	/// <inheritdoc />
	public override bool UpdateWith(ref T value, T update, params string[] exclusions)
	{
		var valueHorizontalLocation = value?.HorizontalLocation;
		var valueVerticalLocation = value?.VerticalLocation;

		return _horizontalComparer.UpdateWith(ref valueHorizontalLocation, update.HorizontalLocation, exclusions)
			| _verticalComparer.UpdateWith(ref valueVerticalLocation, update.VerticalLocation, exclusions);
	}

	/// <inheritdoc />
	public bool UpdateWith(ref T value, THorizontalLocation update, params string[] exclusions)
	{
		var valueHorizontalLocation = value?.HorizontalLocation;
		return _horizontalComparer.UpdateWith(ref valueHorizontalLocation, update, exclusions);
	}

	/// <inheritdoc />
	public bool UpdateWith(ref T value, TVerticalLocation update, params string[] exclusions)
	{
		var valueVerticalLocation = value?.VerticalLocation;
		return _verticalComparer.UpdateWith(ref valueVerticalLocation, update, exclusions);
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(AlwaysTrustSameSource):
			{
				if (_horizontalComparer != null)
				{
					_horizontalComparer.AlwaysTrustSameSource = AlwaysTrustSameSource;
				}
				if (_verticalComparer != null)
				{
					_verticalComparer.AlwaysTrustSameSource = AlwaysTrustSameSource;
				}
				break;
			}
			case nameof(SourceTimeout):
			{
				if (_horizontalComparer != null)
				{
					_horizontalComparer.SourceTimeout = SourceTimeout;
				}
				if (_verticalComparer != null)
				{
					_verticalComparer.SourceTimeout = SourceTimeout;
				}
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	#endregion
}