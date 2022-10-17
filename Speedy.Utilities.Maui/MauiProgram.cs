#region References

using Speedy.Application.Maui;
using Speedy.Application.Maui.Devices.Location;
using Speedy.Devices;
using Speedy.Utilities.Maui.ViewModels;

#endregion

namespace Speedy.Utilities.Maui;

public static class MauiProgram
{
	#region Methods

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var dispatcher = new MauiDispatcher(Dispatcher.GetForCurrentThread());
		var locationProvider = LocationProvider.Create(dispatcher);
		var runtimeInformation = new MauiRuntimeInformation(dispatcher);

		builder.Services.AddScoped<IDispatcher>(_ => dispatcher);
		builder.Services.AddScoped<LocationProvider>(_ => locationProvider);
		builder.Services.AddScoped<RuntimeInformation>(_ => runtimeInformation);
		builder.Services.AddSingleton<ClientViewModel>();
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}

	#endregion
}