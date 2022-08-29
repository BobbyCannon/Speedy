#region References

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Represents a credential for a web client.
	/// </summary>
	public class WebCredential : NetworkCredential
	{
		#region Fields

		private AuthenticationHeaderValue _authenticationHeaderValue;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of the web credential.
		/// </summary>
		public WebCredential() : this(string.Empty, string.Empty)
		{
		}

		/// <summary>
		/// Creates an instance of the web credential.
		/// </summary>
		/// <param name="username"> The username of the credential. </param>
		/// <param name="password"> The password of the credential. </param>
		/// <param name="domain"> An optional domain value. </param>
		public WebCredential(string username, string password, string domain = null) : base(username, password, domain)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the credential as an auth header value.
		/// </summary>
		public AuthenticationHeaderValue AuthenticationHeaderValue => _authenticationHeaderValue ??= new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}")));

		#endregion

		#region Methods

		/// <summary>
		/// Reset the web credential.
		/// </summary>
		public void Reset()
		{
			UserName = string.Empty;
			Password = string.Empty;

			_authenticationHeaderValue = null;
		}

		#endregion
	}
}