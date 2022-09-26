#region References

using Speedy.Devices;
using Speedy.Net;
using Speedy.Plugins.Devices;
using Speedy.Serialization;

#endregion

namespace Speedy.Plugins.Maui
{
    public class MauiSecureVault : SecureVault
    {
        #region Fields

        private string _keyName;

        #endregion

        #region Properties

        private string KeyName => _keyName ??= $"{DeviceFactory.RuntimeInformation.ApplicationName}Credential";

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
            var credential = value?.FromJson<WebCredential>();

            if (credential == null)
            {
                return false;
            }

            Credential.UpdateWith(credential);
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
}