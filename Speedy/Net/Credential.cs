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
/// Represents a credential for a client.
/// </summary>
public class Credential : Bindable, IDisposable, IUpdatable<Credential>
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	public Credential() : this(string.Empty, string.Empty)
	{
	}

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public Credential(IDispatcher dispatcher) : this(string.Empty, string.Empty, dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	/// <param name="username"> The username of the credential. </param>
	/// <param name="password"> The password of the credential. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public Credential(string username, string password, IDispatcher dispatcher = null) : base(dispatcher)
	{
		UserName = username ?? string.Empty;
		SecurePassword = password?.ToSecureString();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Represents the password for the credential.
	/// </summary>
	public string Password
	{
		get => SecurePassword?.ToUnsecureString() ?? string.Empty;
		set => SecurePassword = value?.ToSecureString();
	}

	/// <summary>
	/// Represents the secure password for the credential.
	/// </summary>
	public SecureString SecurePassword { get; set; }

	/// <summary>
	/// Represents the user name for the credential.
	/// </summary>
	public string UserName { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Gets the credential from an authentication header value.
	/// </summary>
	public static Credential FromAuthenticationHeaderValue(AuthenticationHeaderValue headerValue)
	{
		if (!string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
		{
			throw new SecurityException("The authentication header is incorrect schema.");
		}

		var response = new Credential();
		response.Load(headerValue);
		return response;
	}

	/// <summary>
	/// Gets the credential as an authentication header value.
	/// </summary>
	public virtual AuthenticationHeaderValue GetAuthenticationHeaderValue()
	{
		return new AuthenticationHeaderValue("Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{UserName}:{Password}"))
		);
	}

	/// <summary>
	/// Determines if the credentials have been provided.
	/// </summary>
	/// <returns> Returns true if both UserName and Password both is not null or whitespace. </returns>
	public virtual bool HasCredentials()
	{
		return !string.IsNullOrWhiteSpace(UserName)
			&& SecurePassword is { Length: > 0 };
	}

	/// <summary>
	/// Load the credential from an authentication header value.
	/// </summary>
	public virtual void Load(AuthenticationHeaderValue value)
	{
		var credentialBytes = Convert.FromBase64String(value.Parameter);
		var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
		UserName = credentials[0];
		Password = credentials[1];
	}

	/// <summary>
	/// Reset the credential.
	/// </summary>
	public virtual void Reset()
	{
		UserName = string.Empty;
		Password = string.Empty;
		SecurePassword.Clear();
	}

	/// <inheritdoc />
	public bool ShouldUpdate(Credential update)
	{
		return true;
	}

	/// <inheritdoc />
	public bool TryUpdateWith(Credential update, params string[] exclusions)
	{
		return UpdatableExtensions.TryUpdateWith(this, update, exclusions);
	}

	/// <inheritdoc />
	public bool UpdateWith(Credential update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			SecurePassword = update.SecurePassword;
			UserName = update.UserName;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(SecurePassword)), x => x.SecurePassword = update.SecurePassword);
			this.IfThen(_ => !exclusions.Contains(nameof(UserName)), x => x.UserName = update.UserName);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			WebCredential credential => UpdateWith(credential),
			Credential credential => UpdateWith(credential),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		SecurePassword?.Dispose();
	}

	#endregion
}