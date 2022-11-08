#region References

using System;
using Speedy.Devices.Location;
using Xamarin.Essentials;

#endregion

namespace Speedy.Application.Xamarin;

/// <summary>
/// Location provider for the Xamarin Barometer.
/// </summary>
/// <typeparam name="T"> </typeparam>
public class XamarinBarometerVerticalLocationProvider<T> : VerticalLocationProvider<T>
	where T : class, IVerticalLocation, new()
{
	#region Constructors

	/// <summary>
	/// Instantiates the xamarin barometer altitude provider.
	/// </summary>
	/// <param name="dispatcher"> </param>
	public XamarinBarometerVerticalLocationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The Pressure in Hectopascals.
	/// </summary>
	public double Pressure { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void StartListening()
	{
		Barometer.Start(SensorSpeed.Default);
		Barometer.ReadingChanged += BarometerOnReadingChanged;
	}

	/// <inheritdoc />
	public override void StopListening()
	{
		Barometer.Stop();
		Barometer.ReadingChanged -= BarometerOnReadingChanged;
	}

	private void BarometerOnReadingChanged(object sender, BarometerChangedEventArgs e)
	{
		Pressure = e.Reading.PressureInHectopascals;
		var altitudeAboveSeaLevel = Math.Round(44307.69 * (1.0 - Math.Pow(Pressure / 1013.25, 0.190284)) * 10.0) / 10.0;
		LastReadLocation.Altitude = altitudeAboveSeaLevel;
		LastReadLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
	}

	#endregion
}