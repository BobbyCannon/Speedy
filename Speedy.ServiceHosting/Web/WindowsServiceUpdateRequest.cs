namespace Speedy.ServiceHosting.Web
{
	/// <summary>
	/// Represents a request for a chunk of a Windows service update.
	/// </summary>
	public class WindowsServiceUpdateRequest
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the update.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the offset to start reading the update chunk.
		/// </summary>
		public long Offset { get; set; }

		#endregion
	}
}