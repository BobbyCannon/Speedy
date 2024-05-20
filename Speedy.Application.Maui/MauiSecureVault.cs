#region References

using Speedy.Extensions;
using Speedy.Net;
using Speedy.Runtime;
using Speedy.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy.Application.Maui;

public class MauiSecureVault : SecureVault
{
	#region Fields

	private readonly RuntimeInformation _information;
	private string _keyName;

	#endregion

	#region Constructors

	public MauiSecureVault(RuntimeInformation information, IDispatcher dispatcher) : base(dispatcher)
	{
		_information = information;
	}

	#endregion

	#region Properties

	private string KeyName => _keyName ??= GetVaultKey(nameof(Credential));

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void ClearCredential()
	{
		Credential.Reset();
		SecureStorage.Default.Remove(KeyName);
	}

	public override string GetVaultKey(string name)
	{
		return $"{_information.ApplicationName.Replace(" ", "")}.{name}";
	}

	/// <inheritdoc />
	public override async Task<bool> ReadCredentialAsync()
	{
		var value = await SecureStorage.Default.GetAsync(KeyName);
		var credential = value?.FromJson<Credential>();

		if ((credential == null) || (credential.SecurePassword.Length <= 0))
		{
			return false;
		}

		Credential.UpdateWith(credential);
		Credential.ResetHasChanges();

		return true;
	}

	/// <inheritdoc />
	public override bool TryReadData<T>(string key, out T data)
	{
		var storeKey = GetVaultKey(key);
		var value = SecureStorage.Default.GetAsync(storeKey).AwaitResults();
		if (string.IsNullOrEmpty(value))
		{
			data = default;
			return false;
		}

		var response = value.FromJson<T>();
		data = response;
		return true;
	}

	/// <inheritdoc />
	public override bool TryWriteData<T>(string key, T data)
	{
		var storeKey = GetVaultKey(key);
		var response = data.ToJson();
		SecureStorage.Default.SetAsync(storeKey, response).AwaitResults();
		return true;
	}

	/// <inheritdoc />
	public override async Task<bool> WriteCredentialAsync()
	{
		var json = Credential?.ToRawJson();
		if (json == null)
		{
			return false;
		}

		await SecureStorage.Default.SetAsync(KeyName, json);
		return true;
	}

	#endregion
}