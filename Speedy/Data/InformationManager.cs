#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Speedy.Collections;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Data;

/// <summary>
/// Manages a group of information providers and comparers to track a single state of information.
/// </summary>
/// <typeparam name="T"> The type of the value to track. </typeparam>
public abstract class InformationManager<T>
	: Bindable, IInformationProvider
	where T : IUpdatable<T>, new()
{
	#region Fields

	private readonly SortedObservableCollection<IInformationProvider> _providers;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the information manager.
	/// </summary>
	protected InformationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		_providers = new SortedObservableCollection<IInformationProvider>(dispatcher, new OrderBy<IInformationProvider>(x => x.ProviderName));

		// Defaults to empty sources.
		SubProviders = Array.Empty<IInformationProvider>();

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
	public bool HasSubProviders => SubProviders.Any();

	/// <inheritdoc />
	public bool IsEnabled { get; set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; private set; }

	/// <inheritdoc />
	public abstract string ProviderName { get; }

	/// <summary>
	/// The providers for each type.
	/// </summary>
	public ReadOnlyObservableCollection<IInformationProvider> Providers => new ReadOnlyObservableCollection<IInformationProvider>(_providers);

	/// <inheritdoc />
	public virtual IEnumerable<IInformationProvider> SubProviders { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Add a provider of device information to the manager.
	/// </summary>
	/// <param name="provider"> The provider of device information for the type. </param>
	public void Add(IInformationProvider provider)
	{
		_providers.AddOrReplace(
			x => x.ProviderName == provider.ProviderName,
			() =>
			{
				provider.PropertyChanged += ProviderOnPropertyChanged;
				provider.Updated += ProviderOnUpdated;
				return provider;
			},
			existing =>
			{
				if (existing != default)
				{
					existing.PropertyChanged -= ProviderOnPropertyChanged;
					existing.Updated -= ProviderOnUpdated;
				}

				provider.PropertyChanged += ProviderOnPropertyChanged;
				provider.Updated += ProviderOnUpdated;
				return provider;
			});
	}

	/// <inheritdoc />
	public async Task StartMonitoringAsync()
	{
		if (IsMonitoring)
		{
			return;
		}

		foreach (var i in _providers.Where(x => x.IsEnabled))
		{
			await i.StartMonitoringAsync();
		}

		IsMonitoring = true;
	}

	/// <inheritdoc />
	public async Task StopMonitoringAsync()
	{
		foreach (var i in _providers.Where(x => x.IsMonitoring))
		{
			await i.StopMonitoringAsync();
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
				Updated?.Invoke(this, (T) cloneable.ShallowClone());
				return;
			}
			default:
			{
				Updated?.Invoke(this, e.DeepClone());
				break;
			}
		}
	}

	private void ProviderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (sender is not IInformationProvider provider)
		{
			return;
		}

		switch (e.PropertyName)
		{
			case nameof(IInformationProvider.IsEnabled):
			{
				// todo: should this be handled by the provider?
				// ex. what happens if IsMonitoring is false but it is "starting to monitor"?
				if (provider.IsMonitoring && !provider.IsEnabled)
				{
					provider.StopMonitoringAsync();
				}
				else if (!provider.IsMonitoring && provider.IsEnabled)
				{
					provider.StartMonitoringAsync();
				}
				break;
			}
		}
	}

	private void ProviderOnUpdated(object sender, IUpdatable update)
	{
		ProviderUpdated?.Invoke(sender, update);

		var provider = (IInformationProvider) sender;
		if (provider == null)
		{
			// Invalid provider?
			return;
		}

		// Refreshes the BestValue member.
		BestValue.Refresh(update);

		// Notify of the current value change.
		CurrentValue.UpdateWith(update);

		OnUpdated(CurrentValue);
	}

	#endregion

	#region Events

	/// <summary>
	/// Notification of specific provider updates.
	/// </summary>
	public event EventHandler<IUpdatable> ProviderUpdated;

	/// <inheritdoc />
	public event EventHandler<IUpdatable> Updated;

	#endregion
}