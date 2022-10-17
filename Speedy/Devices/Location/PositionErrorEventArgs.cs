using System;

namespace Speedy.Devices.Location
{
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
}