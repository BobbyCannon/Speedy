#region References

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using Speedy.Extensions;

#endregion

namespace Speedy.Net;

/// <summary>
/// Represents a credential for a web client.
/// </summary>
public class WebCredential : Credential, IUpdateable<WebCredential>
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the web credential.
	/// </summary>
	public WebCredential()
		: this(string.Empty, string.Empty)
	{
	}

	/// <summary>
	/// Creates an instance of the web credential.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WebCredential(IDispatcher dispatcher)
		: this(string.Empty, string.Empty, dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the web credential.
	/// </summary>
	/// <param name="username"> The username of the credential. </param>
	/// <param name="password"> The password of the credential. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WebCredential(string username, string password, IDispatcher dispatcher = null)
		: this(username, password?.ToSecureString(), dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the web credential.
	/// </summary>
	/// <param name="username"> The username of the credential. </param>
	/// <param name="password"> The password of the credential. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WebCredential(string username, SecureString password, IDispatcher dispatcher = null)
		: base(username, password, dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets a flag indicating to remember the user.
	/// </summary>
	public bool RememberMe { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets the credential from an authentication header value.
	/// </summary>
	public static WebCredential FromAuthenticationHeaderValue(AuthenticationHeaderValue headerValue)
	{
		if (!string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
		{
			throw new SecurityException("The authentication header is incorrect schema.");
		}

		var response = new WebCredential();
		response.Load(headerValue);
		return response;
	}

	/// <summary>
	/// Reset the web credential.
	/// </summary>
	public override void Reset()
	{
		RememberMe = false;
		base.Reset();
	}

	/// <inheritdoc />
	public bool ShouldUpdate(WebCredential update)
	{
		return true;
	}

	/// <inheritdoc />
	public bool TryUpdateWith(WebCredential update, params string[] exclusions)
	{
		return UpdateableExtensions.TryUpdateWith(this, update, exclusions);
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
			RememberMe = update.RememberMe;
		}
		else
		{
			this.IfThen(_ => !exclusions.Contains(nameof(RememberMe)), x => x.RememberMe = update.RememberMe);
		}

		return base.UpdateWith(update, exclusions);
	}

	/// <inheritdoc />
	public override bool UpdateWith(object update, params string[] exclusions)
	{
		return update switch
		{
			WebCredential credential => UpdateWith(credential, exclusions),
			_ => base.UpdateWith(update, exclusions)
		};
	}

	#endregion
}