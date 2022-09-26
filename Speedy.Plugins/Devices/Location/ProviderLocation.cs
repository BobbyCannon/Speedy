#region References

using Speedy.Devices.Location;
using Speedy.Extensions;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.Plugins.Devices.Location
{
    public class ProviderLocation : GeoLocation, IProviderLocation, ICloneable
	{
		#region Constructors

		public ProviderLocation() : this(null)
		{
		}

		public ProviderLocation(IDispatcher dispatcher, IGeoLocation location, DateTime statusTime)
			: base(dispatcher, location.Latitude, location.Longitude, location.Altitude, location.AltitudeReference)
		{
			StatusTime = statusTime;
		}

		public ProviderLocation(IDispatcher dispatcher, double latitude = 0, double longitude = 0, double altitude = 0, AltitudeReferenceType altitudeReference = AltitudeReferenceType.Unspecified)
			: this(dispatcher, new GeoLocation(latitude, longitude, altitude, altitudeReference), TimeService.UtcNow)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the potential position error radius in meters.
		/// </summary>
		public double Accuracy { get; set; }

		/// <summary>
		/// Gets or sets the potential altitude error range in meters.
		/// </summary>
		/// <remarks> Not supported on Android, will always read 0. </remarks>
		public double AltitudeAccuracy { get; set; }

		/// <summary>
		/// Specifies if the Accuracy value is valid
		/// </summary>
		public bool HasAccuracy { get; set; }

		/// <summary>
		/// Specifies if the Altitude value is valid
		/// </summary>
		public bool HasAltitude { get; set; }

		/// <summary>
		/// Specifies if the Heading value is valid
		/// </summary>
		public bool HasHeading { get; set; }

		/// <summary>
		/// Specifies if the Latitude and Longitude values are valid
		/// </summary>
		public bool HasLatitudeLongitude { get; set; }

		/// <summary>
		/// Specifies if the Speed value is valid
		/// </summary>
		public bool HasSpeed { get; set; }

		/// <summary>
		/// Gets or sets the heading in degrees relative to true North.
		/// </summary>
		public double Heading { get; set; }

		/// <inheritdoc />
		public string SourceName { get; set; }

		/// <summary>
		/// Gets or sets the speed in meters per second.
		/// </summary>
		public double Speed { get; set; }

		/// <summary>
		/// Gets the time the position was read.
		/// </summary>
		public DateTime StatusTime { get; set; }

		#endregion

		#region Methods

		public static void CleanupLocation(IProviderLocation location, string propertyName)
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

		/// <inheritdoc />
		public object DeepClone(int? maxDepth = null)
		{
			return ShallowClone();
		}

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

		public override string ToString()
		{
			return $"{Latitude:N7}°  {Longitude:N7}°  {Altitude:N2}";
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

		#endregion
	}

	public interface IProviderLocation : IGeoLocation
	{
		#region Properties

		/// <summary>
		/// The potential position error radius in meters.
		/// </summary>
		/// <remarks>
		/// https://github.com/jamesmontemagno/GeolocatorPlugin/blob/c18a061b712cad82f14c980e13301a199c3f8012/src/Geolocator.Plugin/Abstractions/Position.shared.cs
		/// </remarks>
		double Accuracy { get; set; }

		/// <summary>
		/// The altitude accuracy.
		/// Note: This currently will always be 0 on Android devices.
		/// </summary>
		double AltitudeAccuracy { get; set; }

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