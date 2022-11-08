#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location.
/// </summary>
public class VerticalLocation : Bindable, IVerticalLocation, IUpdatable<IVerticalLocation>
{
	#region Constructors

	/// <summary>
	/// This constructor is only for serialization, do not actually use.
	/// </summary>
	public VerticalLocation() : this(null)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public VerticalLocation(IDispatcher dispatcher) : this(0, AltitudeReferenceType.Unspecified, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	public VerticalLocation(IMinimalVerticalLocation location, IDispatcher dispatcher = null)
		: this(location.Altitude, location.AltitudeReference, dispatcher)
	{
	}

	/// <summary>
	/// Initialize an instance of the BasicLocation.
	/// </summary>
	/// <param name="altitude"> The default value. </param>
	/// <param name="altitudeReference"> The default value. </param>
	/// <param name="dispatcher"> The default value. </param>
	public VerticalLocation(double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified, IDispatcher dispatcher = null) : base(dispatcher)
	{
		Altitude = altitude;
		AltitudeReference = altitudeReference;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public double Altitude { get; set; }

	/// <inheritdoc />
	public AltitudeReferenceType AltitudeReference { get; set; }

	/// <inheritdoc />
	public bool HasAltitude => this.HasSupportedAltitude();

	/// <inheritdoc />
	public bool HasVerticalAccuracy => this.HasSupportedVerticalAccuracy();

	/// <inheritdoc />
	public bool HasVerticalHeading { get; set; }

	/// <inheritdoc />
	public bool HasVerticalSpeed { get; set; }

	/// <inheritdoc />
	public double VerticalAccuracy { get; set; }

	/// <inheritdoc />
	public AccuracyReferenceType VerticalAccuracyReference { get; set; }

	/// <inheritdoc />
	public LocationFlags VerticalFlags { get; set; }
	
	/// <inheritdoc />
	public double VerticalHeading { get; set; }

	/// <inheritdoc />
	public string VerticalSourceName { get; set; }

	/// <inheritdoc />
	public double VerticalSpeed { get; set; }

	/// <inheritdoc />
	public DateTime VerticalStatusTime { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{Altitude:F3} / {AltitudeReference.GetDisplayName()}";
	}

	/// <summary>
	/// Update the VerticalLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public void UpdateWith(IVerticalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Altitude = update.Altitude;
			AltitudeReference = update.AltitudeReference;
			HasVerticalHeading = update.HasVerticalHeading;
			HasVerticalSpeed = update.HasVerticalSpeed;
			VerticalAccuracy = update.VerticalAccuracy;
			VerticalAccuracyReference = update.VerticalAccuracyReference;
			VerticalFlags = update.VerticalFlags;
			VerticalHeading = update.VerticalHeading;
			VerticalSourceName = update.VerticalSourceName;
			VerticalSpeed = update.VerticalSpeed;
			VerticalStatusTime = update.VerticalStatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalHeading)), x => x.HasVerticalHeading = update.HasVerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(HasVerticalSpeed)), x => x.HasVerticalSpeed = update.HasVerticalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracy)), x => x.VerticalAccuracy = update.VerticalAccuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalAccuracyReference)), x => x.VerticalAccuracyReference = update.VerticalAccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalFlags)), x => x.VerticalFlags = update.VerticalFlags);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalHeading)), x => x.VerticalHeading = update.VerticalHeading);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSourceName)), x => x.VerticalSourceName = update.VerticalSourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalSpeed)), x => x.VerticalSpeed = update.VerticalSpeed);
			this.IfThen(_ => !exclusions.Contains(nameof(VerticalStatusTime)), x => x.VerticalStatusTime = update.VerticalStatusTime);
		}

		//base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		switch (update)
		{
			case VerticalLocation options:
			{
				UpdateWith(options, exclusions);
				return;
			}
			case IVerticalLocation options:
			{
				UpdateWith(options, exclusions);
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