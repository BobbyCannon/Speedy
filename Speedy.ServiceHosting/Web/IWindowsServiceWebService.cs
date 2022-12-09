#region References

using System.Net;
using Speedy.Net;

#endregion

namespace Speedy.ServiceHosting.Web
{
	/// <summary>
	/// Represents the web service interface for updates.
	/// </summary>
	public interface IWindowsServiceWebService
	{
		#region Methods

		/// <summary>
		/// Checks to see if there is an update for the service. The size of the update will be return.
		/// If the service returns an empty name and 0 size if no update is available.
		/// </summary>
		/// <param name="details"> The details of the service that is checking for the update. </param>
		/// <returns> The size of the update. </returns>
		WindowsServiceUpdate CheckForUpdate(WindowsServiceDetails details);

		/// <summary>
		/// Downloads a chuck of the update based on the offset.
		/// </summary>
		/// <param name="request"> The request to download the chuck for.. </param>
		/// <returns> A chuck of the update starting from the update. </returns>
		byte[] DownloadUpdateChunk(WindowsServiceUpdateRequest request);

		/// <summary>
		/// Allows the client to log in to the service. This only has to be implemented by services that require
		/// authentication. If you service does not require authentication then just leave this method not implemented.
		/// </summary>
		/// <param name="credentials"> The credentials to use for authentication. </param>
		void Login(Credential credentials);

		#endregion
	}
}