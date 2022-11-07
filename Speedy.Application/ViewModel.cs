namespace Speedy.Application;

/// <summary>
/// Represents a viewmodel.
/// </summary>
public abstract class ViewModel : Bindable
{
	#region Constructors

	/// <summary>
	/// Instantiates a viewmodel.
	/// </summary>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	protected ViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}