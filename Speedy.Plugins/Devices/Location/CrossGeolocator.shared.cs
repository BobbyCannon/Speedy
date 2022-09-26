namespace Speedy.Plugins.Devices.Location
{
	/// <summary>
	/// Cross platform Location Provider
	/// </summary>
	/// <remarks>
	/// Forked from https://github.com/jamesmontemagno/GeolocatorPlugin
	/// </remarks>
	public class LocationProvider
	{
		#region Fields

		private static readonly Lazy<IGeolocator> _implementation = new Lazy<IGeolocator>(CreateGeolocator, LazyThreadSafetyMode.PublicationOnly);

		#endregion

		#region Properties

		/// <summary>
		/// Current plugin implementation to use
		/// </summary>
		public static IGeolocator Current
		{
			get
			{
				var ret = _implementation.Value;
				if (ret == null)
				{
					throw NotImplementedInReferenceAssembly();
				}
				return ret;
			}
		}

		/// <summary>
		/// Gets if the plugin is supported on the current platform.
		/// </summary>
		public static bool IsSupported => _implementation.Value != null;

		#endregion

		#region Methods

		private static IGeolocator CreateGeolocator()
		{
			#if NETSTANDARD1_0 || NETSTANDARD2_0 || NET6_0 || __MACCATALYST__
			return null;
			#else
			return new GeolocatorImplementation();
			#endif
		}

		private static Exception NotImplementedInReferenceAssembly()
		{
			return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
		}

		#endregion
	}
}