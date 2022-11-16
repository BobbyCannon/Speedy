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
	private readonly T _bestValue;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the device information manager.
	/// </summary>
	protected DeviceInformationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		_providers = new ConcurrentDictionary<Type, IDeviceInformationProvider>();

		CurrentValue = new T();
		_bestValue = new T();
	}

	#endregion

	#region Properties

	/// <summary>
	/// The current final state.
	/// </summary>
	public T CurrentValue { get; }

	/// <summary>
	/// The best value based on each provider.
	/// </summary>
	public T BestValue => _bestValue;

	/// <inheritdoc />
	public Type CurrentValueType => typeof(T);

	/// <inheritdoc />
	public bool HasPermission { get; private set; }

	/// <inheritdoc />
	public bool IsListening { get; private set; }

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
				provider.Changed += ProviderOnChanged;
				provider.Refreshed += ProviderOnRefreshed;
				return provider;
			},
			(_, p) =>
			{
				if (p != null)
				{
					p.Changed -= ProviderOnChanged;
					p.Refreshed -= ProviderOnRefreshed;
				}

				provider.Changed -= ProviderOnChanged;
				provider.Refreshed += ProviderOnRefreshed;
				return provider;
			});
	}

	private void ProviderOnChanged(object sender, object e)
	{
		// Refresh the BestValue member.
		Refresh(e);

		// Change the current value.
		CurrentValue.UpdateWith(e);
		OnChanged(CurrentValue);
	}

	/// <inheritdoc />
	public bool Refresh(object update)
	{
		object bestAsObject = _bestValue;
		return Refresh(ref bestAsObject, update);
	}

	/// <inheritdoc />
	public bool Refresh(ref object value, object update)
	{
		return (value is T tValue)
			&& (update is T tUpdate)
			&& tValue.ShouldUpdate(tUpdate)
			&& tValue.UpdateWith(tUpdate);
	}

	/// <inheritdoc />
	public async Task StartListeningAsync()
	{
		if (IsListening)
		{
			return;
		}

		foreach (var i in _providers)
		{
			await i.Value.StartListeningAsync();
		}

		IsListening = true;
	}

	/// <inheritdoc />
	public async Task StopListeningAsync()
	{
		foreach (var i in _providers)
		{
			await i.Value.StopListeningAsync();
		}

		IsListening = false;
	}

	/// <summary>
	/// Triggers the <see cref="OnChanged" /> event when the device information changes.
	/// </summary>
	/// <param name="e"> The new value. </param>
	protected virtual void OnChanged(T e)
	{
		switch (e)
		{
			case ICloneable<T> cloneableT:
			{
				Changed?.Invoke(this, cloneableT.ShallowClone());
				return;
			}
			case ICloneable cloneable:
			{
				Changed?.Invoke(this, cloneable.ShallowClone());
				return;
			}
			default:
			{
				Changed?.Invoke(this, e);
				break;
			}
		}
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
		if (CurrentValue is not object objectValue)
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
		OnRefreshed(CurrentValue.DeepClone());
	}

	#endregion

	#region Events

	public event EventHandler<object> Changed;
	public event EventHandler<object> Refreshed;

	#endregion
}