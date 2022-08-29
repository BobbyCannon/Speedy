#region References

using System;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Represents the state of a downloaded file.
	/// </summary>
	public class FileDownloaderCompletedEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// True if the download was cancelled.
		/// </summary>
		public bool Cancelled { get; set; }

		/// <summary>
		/// An optional error message if there was an error.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// True if the file was incomplete due to an error.
		/// </summary>
		public bool HasError { get; set; }

		#endregion
	}
}