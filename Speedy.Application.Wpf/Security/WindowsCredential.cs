#region References

using System.Collections.Generic;
using System.Security;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.Application.Wpf.Security
{
	public class WindowsCredential
	{
		#region Constructors

		public WindowsCredential()
		{
			// For serialization, do not remove
		}

		public WindowsCredential(string applicationName, string userName, SecureString password, string comment = null)
			: this(WindowsCredentialManager.CredentialType.Generic, applicationName, userName, password, comment)
		{
		}

		public WindowsCredential(WindowsCredentialManager.CredentialType credentialType, string applicationName, string userName, SecureString password, string comment = null)
		{
			ApplicationName = applicationName;
			Attributes = new Dictionary<string, string>();
			CredentialType = credentialType;
			Comment = comment;
			UserName = userName;
			Password = password;
		}

		#endregion

		#region Properties

		public string ApplicationName { get; }

		public Dictionary<string, string> Attributes { get; }

		public string Comment { get; }

		public WindowsCredentialManager.CredentialType CredentialType { get; }

		public SecureString Password { get; }

		public string UserName { get; }

		#endregion

		#region Methods

		public WebCredential AsWebCredential()
		{
			return new WebCredential(UserName, Password.ToUnsecureString());
		}

		public static WindowsCredential FromUnsecureJson(dynamic t)
		{
			return new WindowsCredential((WindowsCredentialManager.CredentialType) t.Type,
				StringExtensions.FromBase64(t.Name.Value),
				StringExtensions.FromBase64(t.UserName.Value),
				StringExtensions.FromBase64(t.Password.Value).ToSecureString());
		}

		public override string ToString()
		{
			return $"CredentialType: {CredentialType}, ApplicationName: {ApplicationName}, UserName: {UserName}";
		}

		public string ToUnsecureJson()
		{
			return $"{{ \"Name\": \"{ApplicationName.ToBase64()}\", \"Type\": {(int) CredentialType}, \"UserName\": \"{UserName.ToBase64()}\" , \"Password\": \"{Password.ToUnsecureString().ToBase64()}\" }}";
		}

		#endregion
	}
}