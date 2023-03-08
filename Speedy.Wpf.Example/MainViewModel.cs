#region References

using Speedy.Application;
using Speedy.Application.Wpf;
using Speedy.Data;

#endregion

namespace Speedy.Wpf.Example;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		RuntimeInformation = new WpfRuntimeInformation(dispatcher);
	}

	#endregion

	#region Properties

	public RuntimeInformation RuntimeInformation { get; }

	#endregion
}