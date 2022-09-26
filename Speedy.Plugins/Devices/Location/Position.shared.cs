namespace Speedy.Plugins.Devices.Location
{
	/// <summary>
	/// ProviderLocation args
	/// </summary>
	public class PositionEventArgs
		: EventArgs
	{
		#region Constructors

		/// <summary>
		/// ProviderLocation args
		/// </summary>
		/// <param name="position"> </param>
		public PositionEventArgs(IProviderLocation position)
		{
			if (position == null)
			{
				throw new ArgumentNullException("position");
			}

			ProviderLocation = position;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The ProviderLocation
		/// </summary>
		public IProviderLocation ProviderLocation { get; }

		#endregion
	}

	/// <summary>
	/// Location exception
	/// </summary>
	public class GeolocationException
		: Exception
	{
		#region Constructors

		/// <summary>
		/// Location exception
		/// </summary>
		/// <param name="error"> </param>
		public GeolocationException(GeolocationError error)
			: base("A geolocation error occured: " + error)
		{
			if (!Enum.IsDefined(typeof(GeolocationError), error))
			{
				throw new ArgumentException("error is not a valid GelocationError member", "error");
			}

			Error = error;
		}

		/// <summary>
		/// Geolocation error
		/// </summary>
		/// <param name="error"> </param>
		/// <param name="innerException"> </param>
		public GeolocationException(GeolocationError error, Exception innerException)
			: base("A geolocation error occured: " + error, innerException)
		{
			if (!Enum.IsDefined(typeof(GeolocationError), error))
			{
				throw new ArgumentException("error is not a valid GelocationError member", "error");
			}

			Error = error;
		}

		#endregion

		#region Properties

		//The error
		public GeolocationError Error { get; }

		#endregion
	}

	/// <summary>
	/// Error ARgs
	/// </summary>
	public class PositionErrorEventArgs
		: EventArgs
	{
		#region Constructors

		/// <summary>
		/// Constructor for event error args
		/// </summary>
		/// <param name="error"> </param>
		public PositionErrorEventArgs(GeolocationError error)
		{
			Error = error;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The Error
		/// </summary>
		public GeolocationError Error { get; }

		#endregion
	}

	/// <summary>
	/// Error for geolocator
	/// </summary>
	public enum GeolocationError
	{
		/// <summary>
		/// The provider was unable to retrieve any position data.
		/// </summary>
		PositionUnavailable,

		/// <summary>
		/// The app is not, or no longer, authorized to receive location data.
		/// </summary>
		Unauthorized
	}
}