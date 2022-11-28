#region References

using System.Collections.Generic;
using System.Security;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.Application.Wpf.Security;

/// <summary>
/// Represents a credential for windows.
/// </summary>
public class WindowsCredential
{
	#region Constructors

	/// <summary>
	/// Create an instance of a windows credential.
	/// </summary>
	public WindowsCredential()
	{
		// For serialization, do not remove
	}

	/// <summary>
	/// Create an instance of a windows credential.
	/// </summary>
	public WindowsCredential(string applicationName, string userName, SecureString password, string comment = null)
		: this(WindowsCredentialManager.CredentialType.Generic, applicationName, userName, password, comment)
	{
	}

	/// <summary>
	/// Create an instance of a windows credential.
	/// </summary>
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

	/// <summary>
	/// The name of the application.
	/// </summary>
	public string ApplicationName { get; }

	/// <summary>
	/// Attributes for the credential.
	/// </summary>
	public Dictionary<string, string> Attributes { get; }

	/// <summary>
	/// A comment for the credential.
	/// </summary>
	public string Comment { get; }

	/// <summary>
	/// The type of the credential.
	/// </summary>
	public WindowsCredentialManager.CredentialType CredentialType { get; }

	/// <summary>
	/// The password.
	/// </summary>
	public SecureString Password { get; }

	/// <summary>
	/// The UserName.
	/// </summary>
	public string UserName { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Return the secure credential as a web credential. Web credentials are not secure.
	/// </summary>
	/// <returns> </returns>
	public WebCredential AsWebCredential()
	{
		return new WebCredential(UserName, Password.ToUnsecureString());
	}

	/// <summary>
	/// Convert from insecure json.
	/// </summary>
	/// <param name="t"> The dynamic object. </param>
	/// <returns> The credential from json. </returns>
	public static WindowsCredential FromInsecureJson(dynamic t)
	{
		return new WindowsCredential((WindowsCredentialManager.CredentialType) t.Type,
			StringExtensions.FromBase64(t.Name.Value),
			StringExtensions.FromBase64(t.UserName.Value),
			StringExtensions.ToSecureString(StringExtensions.FromBase64(t.Password.Value)));
	}

	/// <summary>
	/// Convert the credential to insecure json.
	/// </summary>
	/// <returns> The json value of the credential. </returns>
	public string ToInsecureJson()
	{
		return $"{{ \"Name\": \"{ApplicationName.ToBase64()}\", \"Type\": {(int) CredentialType}, \"UserName\": \"{UserName.ToBase64()}\" , \"Password\": \"{Password.ToUnsecureString().ToBase64()}\" }}";
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"CredentialType: {CredentialType}, ApplicationName: {ApplicationName}, UserName: {UserName}";
	}

	#endregion
}