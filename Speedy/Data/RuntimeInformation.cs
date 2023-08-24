#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy.Data;

/// <summary>
/// Gets information about the current runtime.
/// </summary>
public abstract class RuntimeInformation : Bindable, ISyncClientDetails
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
	protected RuntimeInformation() : this(null)
	{
	}

	/// <summary>
	/// Creates an instance of the runtime information.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected RuntimeInformation(IDispatcher dispatcher) : base(dispatcher)
	{
		_cache = new ConcurrentDictionary<string, object>();
		_propertyAccessors = new SortedDictionary<string, PropertyInfo>();
		_propertyMethods = new SortedDictionary<string, MethodInfo>();

		SetupPropertyAccessors();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The bitness of the application.
	/// </summary>
	public Bitness ApplicationBitness
	{
		get => GetOrCache<Bitness>(nameof(ApplicationBitness));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The location of the application.
	/// </summary>
	public string ApplicationDataLocation
	{
		get => GetOrCache<string>(nameof(ApplicationDataLocation));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Flag indicating if the application is a development build.
	/// </summary>
	public bool ApplicationIsDevelopmentBuild
	{
		get => GetOrCache<bool>(nameof(ApplicationIsDevelopmentBuild));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// Flag indicating if the application is elevated.
	/// </summary>
	public bool ApplicationIsElevated
	{
		get => GetOrCache<bool>(nameof(ApplicationIsElevated));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The location of the application.
	/// </summary>
	public string ApplicationLocation
	{
		get => GetOrCache<string>(nameof(ApplicationLocation));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The name of the application.
	/// </summary>
	public string ApplicationName
	{
		get => GetOrCache<string>(nameof(ApplicationName));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The version of the application.
	/// </summary>
	public Version ApplicationVersion
	{
		get => GetOrCache<Version>(nameof(ApplicationVersion));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The ID of the device.
	/// </summary>
	public string DeviceId
	{
		get => GetOrCache<string>(nameof(DeviceId));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The name of the device manufacturer.
	/// </summary>
	public string DeviceManufacturer => GetOrCache<string>(nameof(DeviceManufacturer));

	/// <summary>
	/// The model of the device.
	/// </summary>
	public string DeviceModel => GetOrCache<string>(nameof(DeviceModel));

	/// <summary>
	/// The name of the device.
	/// </summary>
	public string DeviceName
	{
		get => GetOrCache<string>(nameof(DeviceName));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The name of the platform.
	/// </summary>
	public DevicePlatform DevicePlatform
	{
		get => GetOrCache<DevicePlatform>(nameof(DevicePlatform));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The bitness of the platform.
	/// </summary>
	public Bitness DevicePlatformBitness
	{
		get => GetOrCache<Bitness>(nameof(DevicePlatformBitness));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The version of the device OS.
	/// </summary>
	public Version DevicePlatformVersion
	{
		get => GetOrCache<Version>(nameof(DevicePlatformVersion));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The type of the device.
	/// </summary>
	public DeviceType DeviceType
	{
		get => GetOrCache<DeviceType>(nameof(DeviceType));
		set => throw new NotImplementedException();
	}

	/// <summary>
	/// The global instance of the runtime information.
	/// </summary>
	public static RuntimeInformation Instance { get; protected set; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets the entry / calling assembly.
	/// </summary>
	public static Assembly GetApplicationAssembly()
	{
		return Assembly.GetEntryAssembly()
			?? Assembly.GetCallingAssembly();
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
	public void Refresh()
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
	/// Override the application name provided by the runtime information.
	/// </summary>
	/// <param name="name"> The name to use as an override. </param>
	public void SetApplicationName(string name)
	{
		SetOverride(nameof(ApplicationName), name);
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
	protected abstract string GetApplicationDataLocation();

	/// <summary>
	/// Get flag indicating if the application is a development build.
	/// </summary>
	protected virtual bool GetApplicationIsDevelopmentBuild()
	{
		return GetApplicationAssembly().IsAssemblyDebugBuild();
	}

	/// <summary>
	/// Get flag indicating if the application is elevated.
	/// </summary>
	protected abstract bool GetApplicationIsElevated();

	/// <summary>
	/// The location of the application.
	/// </summary>
	protected abstract string GetApplicationLocation();

	/// <summary>
	/// The name of the application.
	/// </summary>
	protected abstract string GetApplicationName();

	/// <summary>
	/// The version of the application.
	/// </summary>
	protected abstract Version GetApplicationVersion();

	/// <summary>
	/// The ID of the device.
	/// </summary>
	protected abstract string GetDeviceId();

	/// <summary>
	/// The name of the device manufacturer.
	/// </summary>
	protected abstract string GetDeviceManufacturer();

	/// <summary>
	/// The model of the device.
	/// </summary>
	protected abstract string GetDeviceModel();

	/// <summary>
	/// The name of the device.
	/// </summary>
	protected abstract string GetDeviceName();

	/// <summary>
	/// The name of the platform.
	/// </summary>
	protected abstract DevicePlatform GetDevicePlatform();

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
	protected abstract Version GetDevicePlatformVersion();

	/// <summary>
	/// The type of the device.
	/// </summary>
	protected abstract DeviceType GetDeviceType();

	/// <summary>
	/// Get or cache the value.
	/// </summary>
	/// <typeparam name="T"> </typeparam>
	/// <param name="name"> </param>
	/// <returns> </returns>
	protected T GetOrCache<T>(string name)
	{
		return (T) _cache.GetOrAdd(name, _ => _propertyMethods[name].Invoke(this, null));
	}

	/// <summary>
	/// Set an override for the value.
	/// </summary>
	/// <typeparam name="T"> </typeparam>
	/// <param name="name"> </param>
	/// <param name="value"> </param>
	/// <returns> </returns>
	protected void SetOverride<T>(string name, T value)
	{
		_cache.AddOrUpdate(name, value);
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