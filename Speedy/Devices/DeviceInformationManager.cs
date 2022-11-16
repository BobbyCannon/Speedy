#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Manages a group of information providers and comparers to track a single state of information.
/// </summary>
/// <typeparam name="T"> The type of the value to track. </typeparam>
public abstract class DeviceInformationManager<T>
	: Bindable, IDeviceInformationProvider
	where T : IUpdatable<T>, new()
{
	#region Fields

	private readonly ConcurrentDictionary<Type, IDeviceInformationProvider> _providers;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the device information manager.
	/// </summary>
	protected DeviceInformationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		_providers = new ConcurrentDictionary<Type, IDeviceInformationProvider>();

		CurrentValue = new T();
		BestValue = new T();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The best value based on each provider.
	/// </summary>
	public T BestValue { get; }

	/// <summary>
	/// The current final state.
	/// </summary>
	public T CurrentValue { get; }

	/// <inheritdoc />
	public Type CurrentValueType => typeof(T);

	/// <inheritdoc />
	public bool HasPermission { get; private set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; private set; }

	/// <inheritdoc />
	public abstract string ProviderName { get; }

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
				provider.Updated += ProviderOnUpdated;
				return provider;
			},
			(_, p) =>
			{
				if (p != null)
				{
					p.Updated -= ProviderOnUpdated;
				}

				provider.Updated -= ProviderOnUpdated;
				return provider;
			});
	}

	/// <inheritdoc />
	public bool Refresh(IUpdatable update)
	{
		IUpdatable bestAsObject = BestValue;
		return Refresh(ref bestAsObject, update);
	}

	/// <inheritdoc />
	public bool Refresh(ref IUpdatable value, IUpdatable update)
	{
		return value.ShouldUpdate(update)
			&& value.UpdateWith(update);
	}

	/// <inheritdoc />
	public async Task StartMonitoringAsync()
	{
		if (IsMonitoring)
		{
			return;
		}

		foreach (var i in _providers)
		{
			await i.Value.StartMonitoringAsync();
		}

		IsMonitoring = true;
	}

	/// <inheritdoc />
	public async Task StopMonitoringAsync()
	{
		foreach (var i in _providers)
		{
			await i.Value.StopMonitoringAsync();
		}

		IsMonitoring = false;
	}

	/// <summary>
	/// Triggers the <see cref="OnUpdated" /> event when the device information changes.
	/// </summary>
	/// <param name="e"> The new value. </param>
	protected virtual void OnUpdated(T e)
	{
		switch (e)
		{
			case ICloneable<T> cloneableT:
			{
				Updated?.Invoke(this, cloneableT.ShallowClone());
				return;
			}
			case ICloneable cloneable:
			{
				Updated?.Invoke(this, (IUpdatable) cloneable.ShallowClone());
				return;
			}
			default:
			{
				Updated?.Invoke(this, e.DeepClone());
				break;
			}
		}
	}

	private void ProviderOnUpdated(object sender, IUpdatable update)
	{
		var provider = (IDeviceInformationProvider) sender;
		if (provider == null)
		{
			// Invalid provider?
			return;
		}

		// Just a cast to get to object
		if (CurrentValue is not IUpdatable objectValue)
		{
			return;
		}

		// Try to refresh the Manager's CurrentValue
		if (!provider.Refresh(ref objectValue, update))
		{
			// Current Value was not refresh so do not notify
			return;
		}

		// Notify of the current value with deep clone
		OnUpdated(CurrentValue);
	}

	#endregion

	#region Events

	public event EventHandler<IUpdatable> Updated;

	#endregion
}