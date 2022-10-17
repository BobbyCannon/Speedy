#region References

using Speedy.Application;
using Speedy.Application.Maui.Devices.Location;
using Speedy.Devices;

#endregion

namespace Speedy.Utilities.Maui.ViewModels;

public class ClientViewModel : ViewModel
{
	#region Constructors

	/// <inheritdoc />
	public ClientViewModel(LocationProvider locationProvider, RuntimeInformation runtimeInformation, IDispatcher dispatcher) : base(dispatcher)
	{
		LocationProvider = locationProvider;
		RuntimeInformation = runtimeInformation;
	}

	#endregion

	#region Properties

	public LocationProvider LocationProvider { get; }

	public RuntimeInformation RuntimeInformation { get; }

	#endregion
}