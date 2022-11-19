#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represents a horizontal location.
/// </summary>
public class HorizontalLocation : LocationDeviceInformation, IHorizontalLocation
{
	#region Constants

	/// <summary>
	/// The information ID for horizontal location.
	/// </summary>
	public const string HorizontalLocationInformationId = "A5AB1AC5-9F6D-4B19-A179-5366BEBD1F1D";

	#endregion

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
	public override Guid InformationId => Guid.Parse(HorizontalLocationInformationId);

	/// <inheritdoc />
	public double Latitude { get; set; }

	/// <inheritdoc />
	public double Longitude { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public IHorizontalLocation DeepClone(int? maxDepth = null)
	{
		return ShallowClone();
	}

	/// <inheritdoc />
	public IHorizontalLocation ShallowClone()
	{
		var response = new HorizontalLocation();
		response.UpdateWith(this);
		return response;
	}

	/// <inheritdoc />
	public bool ShouldUpdate(IHorizontalLocation update)
	{
		return base.ShouldUpdate(update);
	}

	/// <summary>
	/// Update the HorizontalLocation with an update.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	public bool UpdateWith(IHorizontalLocation update, params string[] exclusions)
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
			Flags = update.Flags;
			// These values set by flags
			//HasHeading = update.HasHeading;
			//HasSpeed = update.HasSpeed;
			//HasValue = update.HasValue;
			Heading = update.Heading;
			Latitude = update.Latitude;
			Longitude = update.Longitude;
			ProviderName = update.ProviderName;
			SourceName = update.SourceName;
			Speed = update.Speed;
			StatusTime = update.StatusTime;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Accuracy)), x => x.Accuracy = update.Accuracy);
			this.IfThen(_ => !exclusions.Contains(nameof(AccuracyReference)), x => x.AccuracyReference = update.AccuracyReference);
			this.IfThen(_ => !exclusions.Contains(nameof(Flags)), x => x.Flags = update.Flags);
			// These values set by flags
			//this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
			//this.IfThen(_ => !exclusions.Contains(nameof(HasValue)), x => x.HasValue = update.HasValue);
			this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
			this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
			this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
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
			HorizontalLocation options => UpdateWith(options, exclusions),
			IHorizontalLocation options => UpdateWith(options, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
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
/// Represents a horizontal location (lat, long).
/// </summary>
public interface IHorizontalLocation
	: ILocationDeviceInformation,
		IUpdatable<IHorizontalLocation>,
		ICloneable<IHorizontalLocation>,
		IMinimalHorizontalLocation
{
}

/// <summary>
/// Represents a horizontal location (lat, long).
/// </summary>
public interface IMinimalHorizontalLocation : IBindable
{
	#region Properties

	/// <summary>
	/// Ranges between -90 to 90 from North to South
	/// </summary>
	double Latitude { get; set; }

	/// <summary>
	/// Ranges between -180 to 180 from West to East
	/// </summary>
	double Longitude { get; set; }

	#endregion
}