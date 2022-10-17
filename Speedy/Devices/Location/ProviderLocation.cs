#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Protocols;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.Devices.Location
{
	/// <summary>
	/// Represents a location from a LocationProvider.
	/// </summary>
	public class ProviderLocation : GeoLocation, IProviderLocation, ICloneable
	{
		#region Constructors

		/// <summary>
		/// Instantiates a location for a LocationProvider.
		/// </summary>
		public ProviderLocation() : this(null)
		{
		}

		/// <summary>
		/// Instantiates a location for a LocationProvider.
		/// </summary>
		public ProviderLocation(IDispatcher dispatcher, IGeoLocation location, DateTime statusTime)
			: base(dispatcher, location.Latitude, location.Longitude, location.Altitude, location.AltitudeReference)
		{
			StatusTime = statusTime;
		}

		/// <summary>
		/// Instantiates a location for a LocationProvider.
		/// </summary>
		public ProviderLocation(IDispatcher dispatcher, double latitude = 0, double longitude = 0, double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified)
			: this(dispatcher, new GeoLocation(latitude, longitude, altitude, altitudeReference), TimeService.UtcNow)
		{
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public double Accuracy { get; set; }

		/// <inheritdoc />
		public AccuracyReferenceType AccuracyReference { get; set; }

		/// <inheritdoc />
		public double AltitudeAccuracy { get; set; }

		/// <inheritdoc />
		public AccuracyReferenceType AltitudeAccuracyReference { get; set; }

		/// <inheritdoc />
		public bool HasAccuracy { get; set; }

		/// <inheritdoc />
		public bool HasAltitude { get; set; }

		/// <inheritdoc />
		public bool HasHeading { get; set; }

		/// <inheritdoc />
		public bool HasLatitudeLongitude { get; set; }

		/// <inheritdoc />
		public bool HasSpeed { get; set; }

		/// <inheritdoc />
		public double Heading { get; set; }

		/// <inheritdoc />
		public string SourceName { get; set; }

		/// <inheritdoc />
		public double Speed { get; set; }

		/// <inheritdoc />
		public DateTime StatusTime { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public object DeepClone(int? maxDepth = null)
		{
			return ShallowClone();
		}

		/// <inheritdoc />
		public override void OnPropertyChanged(string propertyName)
		{
			CleanupLocation(this, propertyName);
			base.OnPropertyChanged(propertyName);
		}

		/// <inheritdoc />
		public object ShallowClone()
		{
			var response = new ProviderLocation(Dispatcher);
			response.UpdateWith(this);
			return response;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{Latitude:N7}°  {Longitude:N7}°  {Altitude:N2} {AccuracyReference.ToDisplayShortName()}";
		}

		/// <summary>
		/// Update the ProviderLocation with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public void UpdateWith(IProviderLocation update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				Accuracy = update.Accuracy;
				Altitude = update.Altitude;
				AltitudeAccuracy = update.AltitudeAccuracy;
				AltitudeReference = update.AltitudeReference;
				HasAccuracy = update.HasAccuracy;
				HasAltitude = update.HasAltitude;
				HasHeading = update.HasHeading;
				HasLatitudeLongitude = update.HasLatitudeLongitude;
				HasSpeed = update.HasSpeed;
				Heading = update.Heading;
				Latitude = update.Latitude;
				Longitude = update.Longitude;
				SourceName = update.SourceName;
				Speed = update.Speed;
				StatusTime = update.StatusTime;
			}
			else
			{
				this.IfThen(_ => !exclusions.Contains(nameof(Accuracy)), x => x.Accuracy = update.Accuracy);
				this.IfThen(_ => !exclusions.Contains(nameof(Altitude)), x => x.Altitude = update.Altitude);
				this.IfThen(_ => !exclusions.Contains(nameof(AltitudeAccuracy)), x => x.AltitudeAccuracy = update.AltitudeAccuracy);
				this.IfThen(_ => !exclusions.Contains(nameof(AltitudeReference)), x => x.AltitudeReference = update.AltitudeReference);
				this.IfThen(_ => !exclusions.Contains(nameof(HasAccuracy)), x => x.HasAccuracy = update.HasAccuracy);
				this.IfThen(_ => !exclusions.Contains(nameof(HasAltitude)), x => x.HasAltitude = update.HasAltitude);
				this.IfThen(_ => !exclusions.Contains(nameof(HasHeading)), x => x.HasHeading = update.HasHeading);
				this.IfThen(_ => !exclusions.Contains(nameof(HasLatitudeLongitude)), x => x.HasLatitudeLongitude = update.HasLatitudeLongitude);
				this.IfThen(_ => !exclusions.Contains(nameof(HasSpeed)), x => x.HasSpeed = update.HasSpeed);
				this.IfThen(_ => !exclusions.Contains(nameof(Heading)), x => x.Heading = update.Heading);
				this.IfThen(_ => !exclusions.Contains(nameof(Latitude)), x => x.Latitude = update.Latitude);
				this.IfThen(_ => !exclusions.Contains(nameof(Longitude)), x => x.Longitude = update.Longitude);
				this.IfThen(_ => !exclusions.Contains(nameof(SourceName)), x => x.SourceName = update.SourceName);
				this.IfThen(_ => !exclusions.Contains(nameof(Speed)), x => x.Speed = update.Speed);
				this.IfThen(_ => !exclusions.Contains(nameof(StatusTime)), x => x.StatusTime = update.StatusTime);
			}

			//base.UpdateWith(update, exclusions);
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case IProviderLocation options:
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

		private static void CleanupLocation(IProviderLocation location, string propertyName)
		{
			switch (propertyName)
			{
				case nameof(Accuracy):
					if (double.IsNaN(location.Accuracy) || double.IsInfinity(location.Accuracy))
					{
						location.Accuracy = 0;
					}
					break;

				case nameof(Altitude):
					if (double.IsNaN(location.Altitude) || double.IsInfinity(location.Altitude))
					{
						location.Altitude = 0;
					}
					break;

				case nameof(AltitudeAccuracy):
					if (double.IsNaN(location.AltitudeAccuracy) || double.IsInfinity(location.AltitudeAccuracy))
					{
						location.AltitudeAccuracy = 0;
					}
					break;

				case nameof(Heading):
					if (double.IsNaN(location.Heading) || double.IsInfinity(location.Heading))
					{
						location.Heading = 0;
					}
					break;

				case nameof(Latitude):
					if (double.IsNaN(location.Latitude) || double.IsInfinity(location.Latitude))
					{
						location.Latitude = 0;
					}
					break;

				case nameof(Longitude):
					if (double.IsNaN(location.Longitude) || double.IsInfinity(location.Longitude))
					{
						location.Longitude = 0;
					}
					break;

				case nameof(Speed):
					if (double.IsNaN(location.Speed) || double.IsInfinity(location.Speed))
					{
						location.Speed = 0;
					}
					break;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents a provider location.
	/// </summary>
	public interface IProviderLocation : IGeoLocation
	{
		#region Properties

		/// <summary>
		/// The accuracy of the horizontal location. (latitude, longitude)
		/// </summary>
		double Accuracy { get; set; }

		/// <summary>
		/// The reference system for accuracy.
		/// </summary>
		AccuracyReferenceType AccuracyReference { get; set; }

		/// <summary>
		/// The accuracy of the vertical location. (altitude)
		/// </summary>
		double AltitudeAccuracy { get; set; }

		/// <summary>
		/// The reference system for altitude accuracy.
		/// </summary>
		AccuracyReferenceType AltitudeAccuracyReference { get; set; }

		/// <summary>
		/// Specifies if the Accuracy value is valid
		/// </summary>
		bool HasAccuracy { get; set; }

		/// <summary>
		/// Specifies if the Altitude value is valid
		/// </summary>
		bool HasAltitude { get; set; }

		/// <summary>
		/// Specifies if the Heading value is valid
		/// </summary>
		bool HasHeading { get; set; }

		/// <summary>
		/// Specifies if the Latitude and Longitude values are valid
		/// </summary>
		bool HasLatitudeLongitude { get; set; }

		/// <summary>
		/// Specifies if the Speed value is valid
		/// </summary>
		bool HasSpeed { get; set; }

		/// <summary>
		/// The heading of a device.
		/// </summary>
		double Heading { get; set; }

		/// <summary>
		/// The name of the source of the location. Ex. Wifi, GPS, Hardware, Simulated, etc
		/// </summary>
		string SourceName { get; set; }

		/// <summary>
		/// The speed of the device in meters per second.
		/// </summary>
		double Speed { get; set; }

		/// <summary>
		/// The original time of the location was captured.
		/// </summary>
		DateTime StatusTime { get; set; }

		#endregion
	}
}