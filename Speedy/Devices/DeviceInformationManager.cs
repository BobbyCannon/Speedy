#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Speedy.Serialization;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Manages a group of information providers and comparers to track a single state of information.
/// </summary>
/// <typeparam name="T"> The type of the value to track. </typeparam>
public abstract class DeviceInformationManager<T> : Bindable
	where T : new()
{
	#region Fields

	private T _currentValue;

	private readonly ConcurrentDictionary<Type, IDeviceInformationProvider> _providers;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the device information manager.
	/// </summary>
	protected DeviceInformationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		_providers = new ConcurrentDictionary<Type, IDeviceInformationProvider>();

		_currentValue = new T();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The current final state.
	/// </summary>
	public T CurrentValue => _currentValue;

	/// <summary>
	/// The providers for each type.
	/// </summary>
	public IReadOnlyDictionary<Type, IDeviceInformationProvider> Providers => new ReadOnlyDictionary<Type, IDeviceInformationProvider>(_providers);

	#endregion

	#region Methods

	/// <summary>
	/// Add a provider of device information to the manager.
	/// </summary>
	/// <param name="provider"> The provider of device information for the type. </param>
	public void Add(IDeviceInformationProvider provider)
	{
		_providers.AddOrUpdate(provider.CurrentValueType,
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

	/// <summary>
	/// Triggers the <see cref="Refreshed" /> event with the provided value;
	/// </summary>
	/// <param name="e"> The value that was updated. </param>
	protected virtual void OnRefreshed(T e)
	{
		Refreshed?.Invoke(this, e);
	}

	private void ProviderOnRefreshed(object sender, object update)
	{
		var provider = (IDeviceInformationProvider) sender;
		if (provider == null)
		{
			// Invalid provider?
			return;
		}

		// Just a cast to get to object
		if (_currentValue is not object objectValue)
		{
			return;
		}

		// Try to refresh the Manager's CurrentValue
		if (!provider.Refresh(ref objectValue, update))
		{
			// Current Value was not refresh so do not notify
			return;
		}

		// If the value is clone, clone it
		if (objectValue is ICloneable cValue)
		{
			// Notify of the value
			OnRefreshed((T) cValue.ShallowClone());
			return;
		}

		// Notify of the current value with deep clone
		OnRefreshed(_currentValue.DeepClone());
	}

	#endregion

	#region Events

	/// <summary>
	/// An event to notify when the current value is refreshed.
	/// </summary>
	public event EventHandler<T> Refreshed;

	#endregion
}