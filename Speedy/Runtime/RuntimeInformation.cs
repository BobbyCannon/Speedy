#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Speedy.Extensions;
using Speedy.Internal.Windows;
using Speedy.Sync;

#endregion

namespace Speedy.Runtime;

/// <summary>
/// Gets information about the current runtime.
/// </summary>
public class RuntimeInformation : Bindable, IReadOnlyDictionary<string, object>, IRuntimeInformation, ISyncClientDetails
{
	#region Fields

	private readonly ConcurrentDictionary<string, object> _cache;
	private readonly SortedDictionary<string, PropertyInfo> _propertyAccessors;
	private readonly SortedDictionary<string, MethodInfo> _propertyMethods;

	#endregion

	#region Constructors

	/// <summary>
	/// Creates an instance of the runtime information.
	/// </summary>
	public RuntimeInformation() : this(null)
	{
	}

	/// <summary>
	/// Creates an instance of the runtime information.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public RuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
		_cache = new ConcurrentDictionary<string, object>();
		_propertyAccessors = new SortedDictionary<string, PropertyInfo>();
		_propertyMethods = new SortedDictionary<string, MethodInfo>();

		SetupPropertyAccessors();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public Bitness ApplicationBitness
	{
		get => GetOrCache<Bitness>(nameof(ApplicationBitness));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string ApplicationDataLocation
	{
		get => GetOrCache<string>(nameof(ApplicationDataLocation));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string ApplicationFileName => GetOrCache<string>(nameof(ApplicationFileName));

	/// <inheritdoc />
	public string ApplicationFilePath => GetOrCache<string>(nameof(ApplicationFilePath));

	/// <inheritdoc />
	public bool ApplicationIsDevelopmentBuild
	{
		get => GetOrCache<bool>(nameof(ApplicationIsDevelopmentBuild));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public bool ApplicationIsElevated
	{
		get => GetOrCache<bool>(nameof(ApplicationIsElevated));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string ApplicationLocation
	{
		get => GetOrCache<string>(nameof(ApplicationLocation));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string ApplicationName
	{
		get => GetOrCache<string>(nameof(ApplicationName));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Version ApplicationVersion
	{
		get => GetOrCache<Version>(nameof(ApplicationVersion));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public int Count => _cache.Count;

	/// <inheritdoc />
	public string DeviceId
	{
		get => GetOrCache<string>(nameof(DeviceId));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public string DeviceManufacturer => GetOrCache<string>(nameof(DeviceManufacturer));

	/// <inheritdoc />
	public string DeviceModel => GetOrCache<string>(nameof(DeviceModel));

	/// <inheritdoc />
	public string DeviceName
	{
		get => GetOrCache<string>(nameof(DeviceName));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public DevicePlatform DevicePlatform
	{
		get => GetOrCache<DevicePlatform>(nameof(DevicePlatform));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Bitness DevicePlatformBitness
	{
		get => GetOrCache<Bitness>(nameof(DevicePlatformBitness));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Version DevicePlatformVersion
	{
		get => GetOrCache<Version>(nameof(DevicePlatformVersion));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public DeviceType DeviceType
	{
		get => GetOrCache<DeviceType>(nameof(DeviceType));
		set => throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Version DotNetRuntimeVersion => GetOrCache<Version>(nameof(DotNetRuntimeVersion));

	/// <summary>
	/// The global instance of the runtime information.
	/// </summary>
	public static RuntimeInformation Instance { get; protected set; }

	/// <inheritdoc />
	public bool IsShuttingDown { get; private set; }

	/// <inheritdoc />
	public object this[string key] => _cache[key];

	/// <inheritdoc />
	public IEnumerable<string> Keys => _cache.Keys;

	/// <inheritdoc />
	public IEnumerable<object> Values => _cache.Values;

	#endregion

	#region Methods

	/// <inheritdoc />
	public bool ContainsKey(string key)
	{
		return _cache.ContainsKey(key);
	}

	/// <summary>
	/// Gets the entry / calling assembly.
	/// </summary>
	public static Assembly GetApplicationAssembly()
	{
		return Assembly.GetEntryAssembly()
			?? Assembly.GetCallingAssembly();
	}

	/// <inheritdoc />
	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		// ReSharper disable once NotDisposedResourceIsReturned
		return _cache.GetEnumerator();
	}

	/// <summary>
	/// Initialize the runtime information state. Ex. Ensure the paths exists.
	/// </summary>
	public virtual void Initialize()
	{
		new DirectoryInfo(ApplicationDataLocation).SafeCreate();
	}

	/// <summary>
	/// Determines if the platform is Windows.
	/// </summary>
	/// <returns> True if the platform is Windows otherwise false. </returns>
	public bool IsWindows()
	{
		return DevicePlatform == DevicePlatform.Windows;
	}

	/// <summary>
	/// Loads all properties.
	/// </summary>
	public virtual void Refresh()
	{
		foreach (var property in _propertyAccessors)
		{
			property.Value.GetValue(this);
		}
	}

	/// <summary>
	/// Reset the cache.
	/// </summary>
	public void ResetCache()
	{
		_cache.Clear();
	}

	/// <summary>
	/// Reset the cache.
	/// </summary>
	public void ResetCache(string name)
	{
		_cache.TryRemove(name, out _);
	}

	/// <summary>
	/// Set an override for the value.
	/// </summary>
	/// <typeparam name="T"> </typeparam>
	/// <param name="name"> </param>
	/// <param name="value"> </param>
	/// <returns> </returns>
	public void SetOverride<T>(string name, T value)
	{
		_cache.AddOrUpdate(name, value);
	}

	/// <summary>
	/// Mark the runtime as shutting down.
	/// </summary>
	public void Shutdown()
	{
		IsShuttingDown = true;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		var response = new StringBuilder();

		foreach (var cache in _propertyAccessors)
		{
			response.AppendLine($"{cache.Key}: {cache.Value.GetValue(this)}");
		}

		return response.ToString();
	}

	/// <inheritdoc />
	public bool TryGetValue(string key, out object value)
	{
		return _cache.TryGetValue(key, out value);
	}

	/// <summary>
	/// The bitness of the application.
	/// </summary>
	protected virtual Bitness GetApplicationBitness()
	{
		return Environment.Is64BitProcess ? Bitness.X64 : Bitness.X86;
	}

	/// <summary>
	/// The data location of the application.
	/// </summary>
	protected virtual string GetApplicationDataLocation()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		return Path.Combine(localAppData, ApplicationName);
	}

	/// <summary>
	/// The file name of the application.
	/// </summary>
	protected virtual string GetApplicationFileName()
	{
		return Path.GetFileName(GetApplicationFilePath());
	}

	/// <summary>
	/// The file path of the application.
	/// </summary>
	protected virtual string GetApplicationFilePath()
	{
		var path = GetApplicationAssembly().Location;

		if (string.IsNullOrWhiteSpace(path))
		{
			// This is to support PublishSingleFile because Location (above) will be empty
			path = Path.Combine(AppContext.BaseDirectory, GetApplicationName() + ".exe");
		}

		if (path.EndsWith(".dll"))
		{
			path = path.Replace(".dll", ".exe");
		}

		return path;
	}

	/// <summary>
	/// Get flag indicating if the application is a development build.
	/// </summary>
	protected virtual bool GetApplicationIsDevelopmentBuild()
	{
		return GetApplicationAssembly().IsAssemblyDebugBuild();
	}

	/// <summary>
	/// The elevated status of an application.
	/// </summary>
	protected virtual bool GetApplicationIsElevated()
	{
		#pragma warning disable CA1416
		return IsWindows() && (WindowsIdentity.GetCurrent().Owner?.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ?? false);
		#pragma warning restore CA1416
	}

	/// <summary>
	/// The location of the application.
	/// </summary>
	protected virtual string GetApplicationLocation()
	{
		return GetApplicationAssembly().GetAssemblyPath();
	}

	/// <summary>
	/// The name of the application.
	/// </summary>
	protected virtual string GetApplicationName()
	{
		return GetApplicationAssembly()?.GetName().Name;
	}

	/// <summary>
	/// The version of the application.
	/// </summary>
	protected virtual Version GetApplicationVersion()
	{
		return GetApplicationAssembly()?.GetName().Version;
	}

	/// <summary>
	/// The ID of the device.
	/// </summary>
	protected virtual string GetDeviceId()
	{
		return new DeviceId()
			.AddMachineName()
			.AddUserName()
			.AddMachineGuid()
			.AddSystemUuid()
			.AddMotherboardSerialNumber()
			.AddSystemDriveSerialNumber()
			.ToString();
	}

	/// <summary>
	/// The name of the device manufacturer.
	/// </summary>
	protected virtual string GetDeviceManufacturer()
	{
		if (IsWindows())
		{
			#pragma warning disable CA1416
			return new DeviceManufacturerRegistryComponent().GetValue()
				?? new DeviceManufacturerWmiComponent().GetValue();
			#pragma warning restore CA1416
		}

		return string.Empty;
	}

	/// <summary>
	/// The model of the device.
	/// </summary>
	protected virtual string GetDeviceModel()
	{
		if (IsWindows())
		{
			#pragma warning disable CA1416
			return new DeviceModelRegistryComponent().GetValue()
				?? new DeviceModelWmiComponent().GetValue();
			#pragma warning restore CA1416
		}

		return string.Empty;
	}

	/// <summary>
	/// The name of the device.
	/// </summary>
	protected virtual string GetDeviceName()
	{
		return Environment.MachineName;
	}

	/// <summary>
	/// The name of the platform.
	/// </summary>
	protected virtual DevicePlatform GetDevicePlatform()
	{
		#if (NETSTANDARD)
		return DevicePlatform.Windows;
		#else
		if (OperatingSystem.IsWindows())
		{
			return DevicePlatform.Windows;
		}

		if (OperatingSystem.IsAndroid())
		{
			return DevicePlatform.Android;
		}

		if (OperatingSystem.IsIOS())
		{
			return DevicePlatform.IOS;
		}

		if (OperatingSystem.IsLinux())
		{
			return DevicePlatform.Linux;
		}

		return DevicePlatform.Unknown;
		#endif
	}

	/// <summary>
	/// The bitness of the platform.
	/// </summary>
	protected virtual Bitness GetDevicePlatformBitness()
	{
		return Environment.Is64BitOperatingSystem ? Bitness.X64 : Bitness.X86;
	}

	/// <summary>
	/// The version of the device platform version.
	/// </summary>
	protected virtual Version GetDevicePlatformVersion()
	{
		return Environment.OSVersion.Version;
	}

	/// <summary>
	/// The type of the device.
	/// </summary>
	protected virtual DeviceType GetDeviceType()
	{
		#if (NETSTANDARD)
		return DeviceType.Desktop;
		#else
		if (OperatingSystem.IsAndroid()
			|| OperatingSystem.IsIOS())
		{
			return DeviceType.Phone;
		}

		return DeviceType.Desktop;
		#endif
	}

	/// <summary>
	/// The version of the dotnet runtime version.
	/// </summary>
	protected virtual Version GetDotNetRuntimeVersion()
	{
		return Environment.Version;
	}

	/// <summary>
	/// Get or cache the value from the factory.
	/// </summary>
	/// <typeparam name="T"> The type to process. </typeparam>
	/// <param name="name"> The name to represent the value. </param>
	/// <returns> The value by the name provided. </returns>
	protected T GetOrCache<T>(string name)
	{
		return (T) _cache.GetOrAdd(name, _ => _propertyMethods[name].Invoke(this, null));
	}

	/// <summary>
	/// Get or cache the value from the factory.
	/// </summary>
	/// <typeparam name="T"> The type to process. </typeparam>
	/// <param name="name"> The name to represent the value. </param>
	/// <param name="valueFactory"> The factory to create the value. </param>
	/// <returns> The value by the name provided. </returns>
	protected T GetOrCache<T>(string name, Func<T> valueFactory)
	{
		return (T) _cache.GetOrAdd(name, valueFactory.Invoke());
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void SetupPropertyAccessors()
	{
		var type = GetType();
		var properties = type.GetCachedProperties();
		var flags = ReflectionExtensions.DefaultFlags
			.ClearFlag(BindingFlags.Public)
			.SetFlag(BindingFlags.NonPublic);
		var methods = type.GetCachedMethods(flags);

		foreach (var property in properties)
		{
			var method = methods.FirstOrDefault(x => x.Name == $"Get{property.Name}");
			if (method == null)
			{
				continue;
			}

			_propertyAccessors.Add(property.Name, property);
			_propertyMethods.Add(property.Name, method);
		}
	}

	#endregion
}

/// <summary>
/// Gets information about the current runtime.
/// </summary>
public interface IRuntimeInformation : ISyncClientDetails
{
	#region Properties

	/// <summary>
	/// The bitness of the application.
	/// </summary>
	Bitness ApplicationBitness { get; }

	/// <summary>
	/// The location of the application.
	/// </summary>
	string ApplicationDataLocation { get; }

	/// <summary>
	/// The file name of the application.
	/// </summary>
	string ApplicationFileName { get; }

	/// <summary>
	/// The file path of the application.
	/// </summary>
	string ApplicationFilePath { get; }

	/// <summary>
	/// Flag indicating if the application is a development build.
	/// </summary>
	bool ApplicationIsDevelopmentBuild { get; }

	/// <summary>
	/// Flag indicating if the application is elevated.
	/// </summary>
	bool ApplicationIsElevated { get; }

	/// <summary>
	/// The directory of where the application is located.
	/// </summary>
	string ApplicationLocation { get; }

	/// <summary>
	/// The name of the device manufacturer.
	/// </summary>
	string DeviceManufacturer { get; }

	/// <summary>
	/// The model of the device.
	/// </summary>
	string DeviceModel { get; }

	/// <summary>
	/// The bitness of the platform.
	/// </summary>
	Bitness DevicePlatformBitness { get; }

	/// <summary>
	/// The DotNet runtime version.
	/// </summary>
	Version DotNetRuntimeVersion { get; }

	/// <summary>
	/// The flag to track when the application is shutting down.
	/// </summary>
	bool IsShuttingDown { get; }

	#endregion
}