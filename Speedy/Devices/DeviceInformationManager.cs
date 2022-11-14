#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Manages a group of information providers and comparers to track a single state of information.
/// </summary>
/// <typeparam name="T"> The type of the value to track. </typeparam>
public abstract class DeviceInformationManager<T> where T : new()
{
	#region Fields

	private readonly ConcurrentDictionary<Type, IDeviceInformationProvider> _providers;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the device information manager.
	/// </summary>
	protected DeviceInformationManager()
	{
		_providers = new ConcurrentDictionary<Type, IDeviceInformationProvider>();

		CurrentValue = new T();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The current final state.
	/// </summary>
	public T CurrentValue { get; }

	/// <summary>
	/// The providers for each type.
	/// </summary>
	public IReadOnlyDictionary<Type, IDeviceInformationProvider> Providers => new ReadOnlyDictionary<Type, IDeviceInformationProvider>(_providers);

	#endregion

	#region Methods

	/// <summary>
	/// Add a provider of device information to the manager.
	/// </summary>
	/// <typeparam name="T1"> The type the device information provider is for. </typeparam>
	/// <param name="provider"> The provider of device information for the type. </param>
	public void Add<T1>(IDeviceInformationProvider<T1> provider)
		where T1 : new()
	{
		_providers.AddOrUpdate(typeof(T1),
			_ =>
			{
				provider.Refreshed += ProviderOnRefreshed;
				return provider;
			},
			(_, p) =>
			{
				if (p != null)
				{
					p.Refreshed -= ProviderOnRefreshed;
				}
				provider.Refreshed += ProviderOnRefreshed;
				return provider;
			});
	}

	private void ProviderOnRefreshed(object sender, object update)
	{
		var provider = (IDeviceInformationProvider) sender;
		if (provider == null)
		{
			return;
		}

		var currentValue = (object) CurrentValue;
		provider.TryApplyUpdate(ref currentValue, update);
	}

	#endregion
}