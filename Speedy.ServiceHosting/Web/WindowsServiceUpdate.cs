namespace Speedy.ServiceHosting.Web
{
	/// <summary>
	/// Represents the update details for a Windows service.
	/// </summary>
	public class WindowsServiceUpdate
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the update.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the size of the update.
		/// </summary>
		public long Size { get; set; }

		#endregion
	}
}