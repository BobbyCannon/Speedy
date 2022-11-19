#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a vertical location.
/// </summary>
public class VerticalLocation : LocationDeviceInformation, IVerticalLocation
{
	#region Constants

	/// <summary>
	/// The information ID for vertical location.
	/// </summary>
	public const string VerticalLocationInformationId = "2AB3B4A0-A387-409A-BBA3-B74F75972463";

	#endregion

	#region Constructors

	/// <inheritdoc />
	public VerticalLocation() : this(null)
	{
	}

	/// <inheritdoc />
	public VerticalLocation(IDispatcher dispatcher) : this(0, AltitudeReferenceType.Unspecified, dispatcher)
	{
	}

	/// <inheritdoc />
	public VerticalLocation(IMinimalVerticalLocation location, IDispatcher dispatcher = null)
		: this(location.Altitude, location.AltitudeReference, dispatcher)
	{
	}

	/// <inheritdoc />
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
	public override Guid InformationId => Guid.Parse(VerticalLocationInformationId);

	#endregion

	#region Methods

	/// <inheritdoc />
	public IVerticalLocation DeepClone(int? maxDepth = null)
	{
		return ShallowClone();
	}

	/// <inheritdoc />
	public IVerticalLocation ShallowClone()
	{
		var response = new VerticalLocation();
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	public bool ShouldUpdate(IVerticalLocation update)
	{
		return base.ShouldUpdate(update);
	}

	/// <summary>
	/// Update the VerticalLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public bool UpdateWith(IVerticalLocation update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Accuracy = update.Accuracy;
			AccuracyReference = update.AccuracyReference;
			Altitude = update.Altitude;
			AltitudeReference = update.AltitudeReference;
			Flags = update.Flags;
			// These values set by flags
			//HasHeading = update.HasHeading;
			//HasSpeed = update.HasSpeed;
			//HasValue = update.HasValue;
			Heading = update.Heading;
			ProviderName = update.ProviderName;
			SourceName = update.SourceName;
			Speed = update.Speed;
			StatusTime = update.StatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Accuracy)), x => x.Accuracy = update.Accuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(AccuracyReference)), x => x.AccuracyReference = update.AccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
			this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
			this.IfThen(_ => !exclusions.Contains(nameof(Flags)), x => x.Flags = update.Flags);
			// These are handled by flags
			//this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasValue)), x => x.HasValue = update.HasValue);
			this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
			this.IfThen(_ => !exclusions.Contains(nameof(ProviderName)), x => x.ProviderName = update.ProviderName);
			this.IfThen(_ => !exclusions.Contains(nameof(SourceName)), x => x.SourceName = update.SourceName);
			this.IfThen(_ => !exclusions.Contains(nameof(Speed)), x => x.Speed = update.Speed);
			this.IfThen(_ => !exclusions.Contains(nameof(StatusTime)), x => x.StatusTime = update.StatusTime);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			VerticalLocation options => UpdateWith(options, exclusions),
			IVerticalLocation options => UpdateWith(options, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <inheritdoc />
	public override bool UpdateWith(ILocationDeviceInformation update, params string[] exclusions)
	{
		return UpdateWith((object) update, exclusions);
	}

	object ICloneable.DeepClone(int? maxDepth)
	{
		return DeepClone(maxDepth);
	}

	object ICloneable.ShallowClone()
	{
		return ShallowClone();
	}

	#endregion
}

/// <summary>
/// Represents a vertical location (alt, alt ref, acc, acc ref).
/// </summary>
public interface IVerticalLocation
	: ILocationDeviceInformation,
		IUpdatable<IVerticalLocation>,
		ICloneable<IVerticalLocation>,
		IMinimalVerticalLocation
{
}

/// <summary>
/// Represents a vertical location (alt, alt ref).
/// </summary>
public interface IMinimalVerticalLocation : IBindable
{
	#region Properties

	/// <summary>
	/// The altitude of the location
	/// </summary>
	double Altitude { get; set; }

	/// <summary>
	/// The reference type for the altitude value.
	/// </summary>
	AltitudeReferenceType AltitudeReference { get; set; }

	#endregion
}