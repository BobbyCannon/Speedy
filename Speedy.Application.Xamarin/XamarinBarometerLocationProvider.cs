#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Devices.Location;
using Xamarin.Essentials;

#endregion

namespace Speedy.Application.Xamarin;

/// <summary>
/// Location provider for the Xamarin Barometer.
/// </summary>
/// <typeparam name="T"> The vertical location type. </typeparam>
public class XamarinBarometerLocationProvider<T>
	: LocationDeviceInformationProvider<T>
	where T : class, IVerticalLocation, new()
{
	#region Constructors

	/// <summary>
	/// Instantiates the xamarin barometer altitude provider.
	/// </summary>
	/// <param name="dispatcher"> </param>
	public XamarinBarometerLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		CurrentValue.ProviderName = ProviderName;
		CurrentValue.SourceName = "Barometer";
	}

	#endregion

	#region Properties

	/// <summary>
	/// The Pressure in Hectopascals.
	/// </summary>
	public double Pressure { get; set; }

	/// <inheritdoc />
	public sealed override string ProviderName => "Xamarin";

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		return Task.FromResult((T) CurrentValue.ShallowClone());
	}

	/// <inheritdoc />
	public override Task StartMonitoringAsync()
	{
		Barometer.Start(SensorSpeed.Default);
		Barometer.ReadingChanged += BarometerOnReadingChanged;
		IsMonitoring = true;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public override Task StopMonitoringAsync()
	{
		Barometer.Stop();
		Barometer.ReadingChanged -= BarometerOnReadingChanged;
		IsMonitoring = false;
		return Task.CompletedTask;
	}

	private void BarometerOnReadingChanged(object sender, BarometerChangedEventArgs e)
	{
		Pressure = e.Reading.PressureInHectopascals;
		var altitudeAboveSeaLevel = Math.Round(44307.69 * (1.0 - Math.Pow(Pressure / 1013.25, 0.190284)) * 10.0) / 10.0;
		CurrentValue.Altitude = altitudeAboveSeaLevel;
		CurrentValue.AltitudeReference = AltitudeReferenceType.Geoid;
		CurrentValue.StatusTime = TimeService.UtcNow;
		OnUpdated(CurrentValue);
	}

	#endregion
}