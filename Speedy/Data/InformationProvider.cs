#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents a provider of device information.
/// </summary>
public abstract class InformationProvider<T>
	: Comparer<T>, IInformationProvider<T>, IDisposable
	where T : IBindable, IUpdateable<T>, IUpdateable, new()
{
	#region Constructors

	/// <summary>
	/// Represents a provider of device information.
	/// </summary>
	protected InformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		SubProviders = Array.Empty<IInformationProvider>();
		CurrentValue = new T();
		CurrentValue.UpdateDispatcher(dispatcher);
		IsEnabled = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public T CurrentValue { get; }

	/// <inheritdoc />
	public bool HasSubProviders => SubProviders.Any();

	/// <inheritdoc />
	public bool IsEnabled { get; set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; protected set; }

	/// <inheritdoc />
	public abstract string ProviderName { get; }

	/// <inheritdoc />
	public virtual IEnumerable<IInformationProvider> SubProviders { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public virtual Task<T> RefreshAsync()
	{
		return Task.FromResult(new T());
	}

	/// <inheritdoc />
	public abstract Task StartMonitoringAsync();

	/// <inheritdoc />
	public abstract Task StopMonitoringAsync();

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
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
				Updated?.Invoke(this, (IUpdateable) cloneable.ShallowClone());
				return;
			}
			default:
			{
				Updated?.Invoke(this, e);
				break;
			}
		}
	}

	/// <summary>
	/// Update the providers CurrentValue then triggers OnUpdated.
	/// </summary>
	/// <param name="update"> The update. </param>
	protected void UpdateCurrentValue(T update)
	{
		CurrentValue.UpdateWith(update);
		OnUpdated(CurrentValue);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<IUpdateable> Updated;

	#endregion
}