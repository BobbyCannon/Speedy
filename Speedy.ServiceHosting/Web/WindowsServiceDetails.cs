namespace Speedy.ServiceHosting.Web
{
	/// <summary>
	/// Represents windows service requesting an update.
	/// </summary>
	public class WindowsServiceDetails
	{
		#region Properties

		/// <summary>
		/// The name of the service.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The version of the service.
		/// </summary>
		public string Version { get; set; }

		#endregion
	}
}