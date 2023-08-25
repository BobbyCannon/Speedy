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
/// Represents a bearer credential for a client.
/// </summary>
public class TokenCredential : Credential, IUpdateable<TokenCredential>
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	public TokenCredential() : this(string.Empty)
	{
	}

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public TokenCredential(IDispatcher dispatcher) : this(string.Empty, dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the credential.
	/// </summary>
	/// <param name="password"> The token of the credential. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public TokenCredential(string password, IDispatcher dispatcher = null) : base(dispatcher)
	{
		SecurePassword = password?.ToSecureString();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the credential from an authentication header value.
	/// </summary>
	public new static TokenCredential FromAuthenticationHeaderValue(AuthenticationHeaderValue headerValue)
	{
		if (!string.Equals(headerValue.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
		{
			throw new SecurityException("The authentication header is incorrect schema.");
		}

		var response = new TokenCredential();
		response.Load(headerValue);
		return response;
	}

	/// <summary>
	/// Gets the credential as an authentication header value.
	/// </summary>
	public override AuthenticationHeaderValue GetAuthenticationHeaderValue()
	{
		return new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(Password)));
	}

	/// <summary>
	/// Determines if the credentials have been provided. Token credential only requires a password (aka token).
	/// </summary>
	/// <returns> Returns true if the Password is not null or whitespace. </returns>
	public override bool HasCredentials()
	{
		return SecurePassword is { Length: > 0 };
	}

	/// <inheritdoc />
	public override void Load(AuthenticationHeaderValue value)
	{
		Password = value?.Parameter.FromBase64String();
	}

	/// <inheritdoc />
	public bool ShouldUpdate(TokenCredential update)
	{
		return true;
	}

	/// <inheritdoc />
	public bool TryUpdateWith(TokenCredential update, params string[] exclusions)
	{
		return UpdateableExtensions.TryUpdateWith(this, update, exclusions);
	}

	/// <inheritdoc />
	public bool UpdateWith(TokenCredential update, params string[] exclusions)
	{
		// If the update is null then there is nothing to do.
		if (update == null)
		{
			return false;
		}

		// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

		if (exclusions.Length <= 0)
		{
			UserName = update.UserName;
			SecurePassword = update.SecurePassword;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(UserName)), x => x.UserName = update.UserName);
			this.IfThen(_ => !exclusions.Contains(nameof(SecurePassword)), x => x.SecurePassword = update.SecurePassword);
		}

		return true;
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			TokenCredential credential => UpdateWith(credential, exclusions),
			WebCredential credential => UpdateWith(credential, exclusions),
			Credential credential => UpdateWith(credential, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}