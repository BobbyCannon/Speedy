namespace Speedy.Application;

public abstract class ViewModel : Bindable
{
	#region Constructors

	protected ViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion
}