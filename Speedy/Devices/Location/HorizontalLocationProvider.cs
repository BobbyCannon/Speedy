#region References

using System;
using System.Threading.Tasks;
using Speedy.Serialization;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// Represent a provider of horizontal location also known as altitude.
/// </summary>
/// <typeparam name="T"> The type that implements IHorizontalLocation. </typeparam>
public abstract class HorizontalLocationProvider<T> : Bindable, IHorizontalLocationProvider<T>
	where T : class, IHorizontalLocation, ICloneable<T>, new()
{
	#region Constructors

	/// <summary>
	/// Create an instance of the altitude provider.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected HorizontalLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		LastReadLocation = new T();
		LastReadLocation.UpdateDispatcher(dispatcher);
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public virtual bool IsListening { get; protected set; }

	/// <inheritdoc />
	public T LastReadLocation { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public T GetHorizontalLocation()
	{
		return LastReadLocation;
	}

	/// <inheritdoc />
	public abstract Task StartListeningAsync();

	/// <inheritdoc />
	public abstract Task StopListeningAsync();

	/// <summary>
	/// Trigger the horizontal location changed event.
	/// </summary>
	/// <param name="e"> The updated location. </param>
	protected virtual void OnLocationChanged(T e)
	{
		LocationChanged?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<T> LocationChanged;

	#endregion
}