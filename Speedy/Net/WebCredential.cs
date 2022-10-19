#region References

using System;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using Speedy.Extensions;

#endregion

namespace Speedy.Net;

/// <summary>
/// Represents a credential for a web client.
/// </summary>
public class WebCredential : Bindable
{
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
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public WebCredential(IDispatcher dispatcher) : this(string.Empty, string.Empty, dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the web credential.
	/// </summary>
	/// <param name="username"> The username of the credential. </param>
	/// <param name="password"> The password of the credential. </param>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public WebCredential(string username, string password, IDispatcher dispatcher = null) : base(dispatcher)
	{
		UserName = username ?? string.Empty;
		Password = password ?? string.Empty;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Represents the password for the credential.
	/// </summary>
	public string Password
	{
		get => SecurePassword?.ToUnsecureString();
		set => SecurePassword = value?.ToSecureString();
	}

	/// <summary>
	/// Represents the secure password for the credential.
	/// </summary>
	public SecureString SecurePassword { get; set; }

	/// <summary>
	/// Represents the username for the credential.
	/// </summary>
	public string UserName { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets the credential as an auth header value.
	/// </summary>
	public AuthenticationHeaderValue GetAuthenticationHeaderValue()
	{
		return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}")));
	}

	/// <summary>
	/// Determines if the credentials have been provided.
	/// </summary>
	/// <returns> Returns true if both UserName and Password both is not null or whitespace. </returns>
	public bool HasCredentials()
	{
		return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);
	}

	/// <inheritdoc />
	public override void OnPropertyChanged(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(UserName):
			case nameof(Password):
			{
				OnPropertyChanged(nameof(AuthenticationHeaderValue));
				break;
			}
		}

		base.OnPropertyChanged(propertyName);
	}

	/// <summary>
	/// Reset the web credential.
	/// </summary>
	public void Reset()
	{
		UserName = string.Empty;
		Password = string.Empty;
	}

	/// <summary>
	/// Update with the provided credential.
	/// </summary>
	/// <param name="credential"> The credential to update with. </param>
	public void UpdateWith(WebCredential credential)
	{
		UserName = credential.UserName;
		Password = credential.Password;
	}

	/// <inheritdoc />
	public override void UpdateWith(object update, params string[] exclusions)
	{
		if (update is WebCredential webCredential)
		{
			UpdateWith(webCredential);
			return;
		}

		base.UpdateWith(update, exclusions);
	}

	#endregion
}