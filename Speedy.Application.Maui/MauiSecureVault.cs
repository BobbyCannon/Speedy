#region References

using Speedy.Data;
using Speedy.Net;
using Speedy.Serialization;

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

	private string KeyName => _keyName ??= $"{_information.ApplicationName.Replace(" ", "")}Credential";

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void ClearCredential()
	{
		Credential.Reset();
		SecureStorage.Default.Remove(KeyName);
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