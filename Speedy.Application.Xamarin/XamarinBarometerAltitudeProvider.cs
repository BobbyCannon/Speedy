#region References

using System;
using Speedy.Devices.Location;
using Xamarin.Essentials;

#endregion

namespace Speedy.Application.Xamarin;

public class XamarinBarometerAltitudeProvider<T> : AltitudeProvider<T>
	where T : class, IVerticalLocation, new()
{
	#region Constructors

	public XamarinBarometerAltitudeProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	public override void StartListening()
	{
		Barometer.Start(SensorSpeed.Default);
		Barometer.ReadingChanged += BarometerOnReadingChanged;
	}

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