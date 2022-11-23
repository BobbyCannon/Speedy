#region References

using System.Threading.Tasks;
using Speedy.Net;

#endregion

namespace Speedy.Data;

/// <summary>
/// The vault to store credential securely.
/// </summary>
public abstract class SecureVault : Bindable
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the vault.
	/// </summary>
	public SecureVault() : this(null)
	{
	}

	/// <summary>
	/// Creates an instance of the vault.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public SecureVault(IDispatcher dispatcher) : base(dispatcher)
	{
		Credential = new WebCredential(dispatcher);
	}

	#endregion

	#region Properties

	/// <summary>
	/// The credential for the vault.
	/// </summary>
	public WebCredential Credential { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Clears the credential from the vault.
	/// </summary>
	public abstract void ClearCredential();

	/// <summary>
	/// Gets the stored credential from the vault.
	/// </summary>
	/// <returns> Return true if the credential was read otherwise false. </returns>
	public abstract Task<bool> ReadCredentialAsync();

	/// <summary>
	/// Writes a credential to the vault.
	/// </summary>
	/// <returns> Return true if the credential was written otherwise false. </returns>
	public abstract Task<bool> WriteCredentialAsync();

	#endregion
}