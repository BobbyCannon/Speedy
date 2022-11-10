#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location.
/// </summary>
public class HorizontalLocation : CloneableBindable<HorizontalLocation, IHorizontalLocation>, IHorizontalLocation
{
	#region Constructors

	/// <inheritdoc />
	public HorizontalLocation() : this(null)
	{
	}

	/// <inheritdoc />
	public HorizontalLocation(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool HasHorizontalAccuracy => this.HasSupportedHorizontalAccuracy();

	/// <inheritdoc />
	public bool HasHorizontalHeading { get; set; }

	/// <inheritdoc />
	public bool HasHorizontalSpeed { get; set; }

	/// <inheritdoc />
	public double HorizontalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType HorizontalAccuracyReference { get; set; }

	/// <inheritdoc />
	public LocationFlags HorizontalFlags { get; set; }

	/// <inheritdoc />
	public double HorizontalHeading { get; set; }

	/// <inheritdoc />
	public string HorizontalSourceName { get; set; }

	/// <inheritdoc />
	public double HorizontalSpeed { get; set; }

	/// <inheritdoc />
	public DateTime HorizontalStatusTime { get; set; }

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Update the HorizontalLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public override void UpdateWith(HorizontalLocation update, params string[] exclusions)
	{
		UpdateWith(update, exclusions);
	}

	/// <summary>
	/// Update the HorizontalLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public void UpdateWith(IHorizontalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			HasHorizontalHeading = update.HasHorizontalHeading;
			HasHorizontalSpeed = update.HasHorizontalSpeed;
			HorizontalAccuracy = update.HorizontalAccuracy;
			HorizontalAccuracyReference = update.HorizontalAccuracyReference;
			HorizontalFlags = update.HorizontalFlags;
			HorizontalHeading = update.HorizontalHeading;
			HorizontalSourceName = update.HorizontalSourceName;
			HorizontalSpeed = update.HorizontalSpeed;
			HorizontalStatusTime = update.HorizontalStatusTime;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalHeading)), x => x.HasHorizontalHeading = update.HasHorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasHorizontalSpeed)), x => x.HasHorizontalSpeed = update.HasHorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracy)), x => x.HorizontalAccuracy = update.HorizontalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalAccuracyReference)), x => x.HorizontalAccuracyReference = update.HorizontalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalFlags)), x => x.HorizontalFlags = update.HorizontalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalHeading)), x => x.HorizontalHeading = update.HorizontalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSourceName)), x => x.HorizontalSourceName = update.HorizontalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalSpeed)), x => x.HorizontalSpeed = update.HorizontalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(HorizontalStatusTime)), x => x.HorizontalStatusTime = update.HorizontalStatusTime);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
		}

		//base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		switch (update)
		{
			case HorizontalLocation location:
			{
				UpdateWith(location, exclusions);
				return;
			}
			case IHorizontalLocation location:
			{
				UpdateWith(location, exclusions);
				return;
			}
			default:
			{
				base.UpdateWith(update, exclusions);
				return;
			}
		}
	}

	#endregion
}