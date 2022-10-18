#region References

using System;
using System.Threading.Tasks;
using Speedy.Devices;
using Speedy.Serialization;

#endregion

namespace Speedy.Application.Wpf;

public class WindowsSecureVault : SecureVault
{
	#region Fields

	private readonly RuntimeInformation _information;

	private string _keyName;

	#endregion

	#region Constructors

	public WindowsSecureVault(RuntimeInformation information, IDispatcher dispatcher) : base(dispatcher)
	{
		_information = information;
	}

	#endregion

	#region Properties

	private string KeyName => _keyName ??= $"{_information.ApplicationName}Credential";

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void ClearCredential()
	{
		Credential.Reset();
		//SecureStorage.Default.Remove(KeyName);
	}

	/// <inheritdoc />
	public override Task<bool> ReadCredentialAsync()
	{
		if (!WpfRuntimeInformation.Instance.IsWindows())
		{
			throw new NotSupportedException();
		}

		return Task.FromResult(true);
	}

	/// <inheritdoc />
	public override Task<bool> WriteCredentialAsync()
	{
		var json = Credential?.ToRawJson();
		if (json == null)
		{
			return Task.FromResult(false);
		}

		//await SecureStorage.Default.SetAsync(KeyName, json);
		return Task.FromResult(true);
	}

	#endregion
}