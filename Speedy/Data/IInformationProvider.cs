#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents a provider of information.
/// </summary>
public interface IInformationProvider<out T> : IInformationProvider
	where T : IUpdatable<T>, new()
{
	#region Properties

	/// <summary>
	/// Represents the current value.
	/// </summary>
	public T CurrentValue { get; }

	#endregion
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IInformationProvider : IUpdatable, INotifyPropertyChanged
{
	#region Properties

	/// <summary>
	/// Returns true if there are source providers available.
	/// </summary>
	bool HasSubProviders { get; }

	/// <summary>
	/// Determines if the provider is enabled.
	/// </summary>
	bool IsEnabled { get; set; }

	/// <summary>
	/// Determines if the provider is listening.
	/// </summary>
	bool IsMonitoring { get; }

	/// <summary>
	/// Gets the name of the provider.
	/// </summary>
	string ProviderName { get; }

	/// <summary>
	/// An optional set of sources for the information provider.
	/// </summary>
	IEnumerable<IInformationProvider> SubProviders { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Start monitoring for device information changes.
	/// </summary>
	Task StartMonitoringAsync();

	/// <summary>
	/// Stop monitoring for device information changes.
	/// </summary>
	Task StopMonitoringAsync();

	#endregion

	#region Events

	/// <summary>
	/// An event to notify when the device information was updated.
	/// </summary>
	event EventHandler<IUpdatable> Updated;

	#endregion
}