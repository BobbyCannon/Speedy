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

namespace Speedy.Devices;

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
	/// <param name="dispatcher"> An optional dispatcher. </param>
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
	public string DeviceName => GetOrCache<string>(nameof(DeviceName));

	/// <summary>
	/// The name of the platform.
	/// </summary>
	public DevicePlatform DevicePlatform
	{
		get => GetOrCache<DevicePlatform>(nameof(DevicePlatform));
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

	#endregion

	#region Methods

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
	/// The type of the device.
	/// </summary>
	protected abstract DeviceType GetDeviceType();

	private T GetOrCache<T>(string name)
	{
		return (T) _cache.GetOrAdd(name, _ => _propertyMethods[name].Invoke(this, null));
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