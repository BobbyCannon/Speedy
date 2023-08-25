#region References

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Speedy.Application.Maui;

#endregion

namespace Speedy.Samples.Maui;

public static class MauiProgram
{
	#region Methods

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var dispatcher = MauiDispatcher.Initialize(Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread());
		builder.Services.AddScoped<IDispatcher>(_ => dispatcher);
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();

		return builder.Build();
	}

	#endregion
}