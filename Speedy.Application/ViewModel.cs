namespace Speedy.Application;

/// <summary>
/// Represents a viewmodel that has an ID.
/// </summary>
/// <typeparam name="T"> The type for the ID key. </typeparam>
public abstract class ViewModel<T> : ViewModel
{
	#region Constructors

	/// <summary>
	/// Instantiates a viewmodel.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected ViewModel(IDispatcher dispatcher = null) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets the ID of the entity.
	/// </summary>
	public T Id { get; set; }

	#endregion
}

/// <summary>
/// Represents a viewmodel.
/// </summary>
public abstract class ViewModel : Bindable
{
	#region Constructors

	/// <summary>
	/// Instantiates a viewmodel.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected ViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}