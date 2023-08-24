#region References

using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Speedy.Data;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy.Application.Wpf;

/// <summary>
/// A secure vault for WPF applications.
/// </summary>
public class WpfSecureVault : SecureVault
{
	#region Fields

	private readonly DirectoryInfo _dataDirectory;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the WPF secure vault.
	/// </summary>
	/// <param name="information"> The runtime information. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WpfSecureVault(RuntimeInformation information, IDispatcher dispatcher = null) : this(null, information.ApplicationDataLocation, dispatcher)
	{
	}

	/// <summary>
	/// Create an instance of the WPF secure vault.
	/// </summary>
	/// <param name="dataDirectory"> The data directory to store the vault entries. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WpfSecureVault(DirectoryInfo dataDirectory, IDispatcher dispatcher = null) : this(null, dataDirectory.FullName, dispatcher)
	{
	}

	/// <summary>
	/// Create an instance of the WPF secure vault.
	/// </summary>
	/// <param name="dataDirectory"> The data directory to store the vault entries. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WpfSecureVault(string dataDirectory, IDispatcher dispatcher = null) : this(null, dataDirectory, dispatcher)
	{
	}
	
	/// <summary>
	/// Create an instance of the WPF secure vault.
	/// </summary>
	/// <param name="credential"> The default credential.  </param>
	/// <param name="dataDirectory"> The data directory to store the vault entries. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public WpfSecureVault(Credential credential, string dataDirectory, IDispatcher dispatcher = null) : base(credential, dispatcher)
	{
		_dataDirectory = new DirectoryInfo(dataDirectory);
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void ClearCredential()
	{
		var keyFileInfo = GetVaultKeyFilePath(nameof(Credential));
		keyFileInfo.SafeDelete();

		Credential.Reset();
	}

	/// <inheritdoc />
	public override string GetVaultKey(string name)
	{
		return $"{name}.vault";
	}

	/// <summary>
	/// Get the file path of the vault key.
	/// </summary>
	/// <param name="name"> The name of the object being stored. </param>
	/// <returns> The vault key file info for the provided name. </returns>
	public FileInfo GetVaultKeyFilePath(string name)
	{
		return new FileInfo(Path.Combine(_dataDirectory.FullName, "Vault", GetVaultKey(name)));
	}

	/// <inheritdoc />
	public override Task<bool> ReadCredentialAsync()
	{
		var result = TryReadData<Credential>(nameof(Credential), out var credential);
		if (!result)
		{
			return Task.FromResult(false);
		}

		Credential.UpdateWith(credential);

		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public override bool TryReadData<T>(string name, out T data)
	{
		var keyFileInfo = GetVaultKeyFilePath(name);
		if (!keyFileInfo.Exists)
		{
			data = default;
			return false;
		}

		try
		{
			var encrypted = File.ReadAllBytes(keyFileInfo.FullName);
			var jsonBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
			var json = Encoding.Default.GetString(jsonBytes);
			data = json.FromJson<T>();
			return true;
		}
		catch
		{
			data = default;
			return false;
		}
	}

	/// <inheritdoc />
	public override bool TryWriteData<T>(string name, T data)
	{
		try
		{
			var keyFileInfo = GetVaultKeyFilePath(name);
			keyFileInfo.Directory.SafeCreate();
			var json = data.ToJson();
			var jsonBytes = Encoding.Default.GetBytes(json);
			var encrypted = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);
			File.WriteAllBytes(keyFileInfo.FullName, encrypted);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <inheritdoc />
	public override Task<bool> WriteCredentialAsync()
	{
		var json = Credential?.ToRawJson();
		return Task.FromResult((json != null) && TryWriteData(nameof(Credential), Credential));
	}

	#endregion
}