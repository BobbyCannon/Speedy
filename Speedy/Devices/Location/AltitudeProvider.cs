namespace Speedy.Devices.Location;

/// <summary>
/// Represent a provider of vertical location also known as altitude.
/// </summary>
/// <typeparam name="T"> The type that implements IVerticalLocation. </typeparam>
public abstract class AltitudeProvider<T> : Bindable, IVerticalLocationProvider<T>
	where T : class, IVerticalLocation, new()
{
	#region Constructors

	/// <summary>
	/// Create an instance of the altitude provider.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected AltitudeProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		LastReadLocation = new T();
		LastReadLocation.UpdateDispatcher(dispatcher);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets if the provider is listening for altitude changes.
	/// </summary>
	public virtual bool IsListening { get; protected set; }

	/// <summary>
	/// 
	/// </summary>
	public T LastReadLocation { get; }

	/// <summary>
	/// 
	/// </summary>
	public double Pressure { get; protected set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public T GetVerticalLocation()
	{
		return LastReadLocation;
	}

	/// <summary>
	/// Start listening for altitude changes.
	/// </summary>
	public abstract void StartListening();

	/// <summary>
	/// Stop listening for altitude changes.
	/// </summary>
	public abstract void StopListening();

	#endregion
}