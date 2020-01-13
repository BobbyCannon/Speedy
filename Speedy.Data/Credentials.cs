namespace Speedy.Data
{
	/// <summary>
	/// Represents credentials for a user login.
	/// </summary>
	public class Credentials
	{
		#region Properties

		/// <summary>
		/// Gets or sets the user name.
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets a flag indicating to remember the user.
		/// </summary>
		public bool RememberMe { get; set; }

		#endregion
	}
}