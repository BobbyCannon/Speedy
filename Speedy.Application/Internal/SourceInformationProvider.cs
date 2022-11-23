#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Speedy.Data;

#endregion

namespace Speedy.Application.Internal;

/// <summary>
/// The source of a location provider.
/// </summary>
internal class SourceInformationProvider : Bindable, IInformationProvider
{
	#region Constructors

	/// <summary>
	/// Instantiate an instance of a location provider source.
	/// </summary>
	public SourceInformationProvider() : this(null)
	{
	}

	/// <summary>
	/// Instantiate an instance of a location provider source.
	/// </summary>
	public SourceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		SubProviders = Array.Empty<IInformationProvider>();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool HasSubProviders => SubProviders.Any();

	/// <inheritdoc />
	public bool IsEnabled { get; set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; set; }

	/// <inheritdoc />
	public string ProviderName { get; set; }

	/// <inheritdoc />
	public IEnumerable<IInformationProvider> SubProviders { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public Task StartMonitoringAsync()
	{
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task StopMonitoringAsync()
	{
		return Task.CompletedTask;
	}

	#endregion

	#region Events

	public event EventHandler<IUpdatable> Updated;

	#endregion
}