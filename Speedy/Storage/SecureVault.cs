#region References

using System.Threading.Tasks;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.Storage;

/// <summary>
/// The vault to store credential securely.
/// </summary>
public abstract class SecureVault : Bindable
{
	#region Constructors

	/// <summary>
	/// Creates an instance of the vault.
	/// </summary>
	protected SecureVault() : this(null)
	{
	}

	/// <summary>
	/// Creates an instance of the vault.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected SecureVault(IDispatcher dispatcher) : this(new Credential(dispatcher), dispatcher)
	{
	}

	/// <summary>
	/// Creates an instance of the vault.
	/// </summary>
	/// <param name="credential"> The default credential. Allows setting web or token as base credential type. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected SecureVault(Credential credential, IDispatcher dispatcher) : base(dispatcher)
	{
		if (credential == null)
		{
			Credential = new Credential(dispatcher);
			return;
		}

		var clone = credential.ShallowClone();
		clone.UpdateDispatcher(dispatcher);
		Credential = clone;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The credential for the vault.
	/// </summary>
	public Credential Credential { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Clears the credential from the vault.
	/// </summary>
	public abstract void ClearCredential();

	/// <summary>
	/// Calculate a vault key by the provide name.
	/// </summary>
	/// <param name="name"> The name of the object being stored. </param>
	/// <returns> The vault key for the provided name. </returns>
	public abstract string GetVaultKey(string name);

	/// <summary>
	/// Gets the stored credential from the vault.
	/// </summary>
	/// <returns> Return true if the credential was read otherwise false. </returns>
	public bool ReadCredential()
	{
		return ReadCredentialAsync().AwaitResults();
	}

	/// <summary>
	/// Gets the stored credential from the vault.
	/// </summary>
	/// <returns> Return true if the credential was read otherwise false. </returns>
	public abstract Task<bool> ReadCredentialAsync();

	/// <summary>
	/// Gets the stored data from the vault.
	/// </summary>
	/// <param name="name"> The name of the data to read. </param>
	/// <param name="data"> The data read if the key was found. </param>
	/// <returns> Return true if the data was read otherwise false. </returns>
	public abstract bool TryReadData<T>(string name, out T data);

	/// <summary>
	/// Writes the data to the vault.
	/// </summary>
	/// <param name="name"> The name of the data to write. </param>
	/// <param name="data"> The data to write. </param>
	/// <returns> Return true if the data was written otherwise false. </returns>
	public abstract bool TryWriteData<T>(string name, T data);

	/// <summary>
	/// Writes a credential to the vault.
	/// </summary>
	/// <returns> Return true if the credential was written otherwise false. </returns>
	public bool WriteCredential()
	{
		return WriteCredentialAsync().AwaitResults();
	}

	/// <summary>
	/// Writes a credential to the vault.
	/// </summary>
	/// <returns> Return true if the credential was written otherwise false. </returns>
	public abstract Task<bool> WriteCredentialAsync();

	#endregion
}