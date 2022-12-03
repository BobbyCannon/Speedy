#region References

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using Speedy.Extensions;

#endregion

namespace Speedy.Net;

/// <summary>
/// Represents a credential for a web client.
/// </summary>
public class WebCredential : Bindable, IUpdatable<WebCredential>
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
	/// Gets or sets a flag indicating to remember the user.
	/// </summary>
	public bool RememberMe { get; set; }

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
	/// Gets the credential from an authentication header value.
	/// </summary>
	public static WebCredential FromAuthenticationHeaderValue(AuthenticationHeaderValue value)
	{
		var credentialBytes = Convert.FromBase64String(value.Parameter);
		var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
		var username = credentials[0];
		var password = credentials[1];
		return new WebCredential(username, password);
	}

	/// <summary>
	/// Gets the credential as an authentication header value.
	/// </summary>
	public AuthenticationHeaderValue GetAuthenticationHeaderValue()
	{
		return new AuthenticationHeaderValue("Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"))
		);
	}

	/// <summary>
	/// Determines if the credentials have been provided.
	/// </summary>
	/// <returns> Returns true if both UserName and Password both is not null or whitespace. </returns>
	public bool HasCredentials()
	{
		return !string.IsNullOrWhiteSpace(UserName)
			&& !string.IsNullOrWhiteSpace(Password);
	}

	/// <summary>
	/// Reset the web credential.
	/// </summary>
	public void Reset()
	{
		UserName = string.Empty;
		Password = string.Empty;
		RememberMe = false;
	}

	/// <inheritdoc />
	public bool ShouldUpdate(WebCredential update)
	{
		return true;
	}

	/// <inheritdoc />
	public bool TryUpdateWith(WebCredential update, params string[] exclusions)
	{
		return UpdatableExtensions.TryUpdateWith(this, update, exclusions);
	}

	/// <inheritdoc />
	public bool UpdateWith(WebCredential update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			Password = update.Password;
			RememberMe = update.RememberMe;
			UserName = update.UserName;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(Password)), x => x.Password = update.Password);
			this.IfThen(_ => !exclusions.Contains(nameof(RememberMe)), x => x.RememberMe = update.RememberMe);
			this.IfThen(_ => !exclusions.Contains(nameof(UserName)), x => x.UserName = update.UserName);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			WebCredential webCredential => UpdateWith(webCredential),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}