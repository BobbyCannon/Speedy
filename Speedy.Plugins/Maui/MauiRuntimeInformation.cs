#region References

using Speedy.Devices;

#endregion

namespace Speedy.Plugins.Maui
{
    public abstract class MauiRuntimeInformation : IRuntimeInformation
    {
        #region Fields

        private Version _applicationVersion;

        #endregion

        #region Properties

        /// <inheritdoc />
        public string ApplicationName => AppInfo.Current.Name;

        /// <inheritdoc />
        public Version ApplicationVersion => _applicationVersion ??= Version.Parse(AppInfo.Current.Version.ToString(3));

        /// <inheritdoc />
        public abstract string DeviceId { get; }

        /// <inheritdoc />
        public string DeviceManufacturer => DeviceInfo.Current.Manufacturer;

        /// <inheritdoc />
        public string DeviceModel => DeviceInfo.Current.Model;

        /// <inheritdoc />
        public string DeviceName => DeviceInfo.Current.Name;

        /// <inheritdoc />
        public string DeviceType => DeviceInfo.Current.DeviceType.ToString();

        /// <inheritdoc />
        public string PlatformName => DeviceInfo.Current.Platform.ToString();

        #endregion
    }
}