#region References

using System;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Speedy.Application;
using Speedy.Application.Xamarin;
using Speedy.Devices.Location;

#endregion

namespace Speedy.Samples.Xamarin
{
	public class MainViewModel : ViewModel
	{
		#region Constructors

		public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
		{
			LocationProvider = new XamarinLocationProvider<Location, LocationProviderSettings>(dispatcher);
			LocationProvider.PositionChanged += LocationProviderOnPositionChanged;
			var plotModel = new PlotModel
			{
				Title = "Altitude History",
				TextColor = OxyColors.White,
				PlotAreaBorderColor = OxyColors.White,
				TitleColor = OxyColors.White
			};

			plotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom });
			plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, MinorTickSize = 10 });

			var series = new LineSeries
			{
				MarkerType = MarkerType.Circle,
				MarkerSize = 4,
				MarkerStroke = OxyColors.White
			};

			plotModel.Series.Add(series);
			AltitudeChart = plotModel;
		}

		#endregion

		#region Properties

		public PlotModel AltitudeChart { get; }

		public XamarinLocationProvider<Location, LocationProviderSettings> LocationProvider { get; }

		#endregion

		#region Methods

		private void LocationProviderOnPositionChanged(object sender, Location e)
		{
			var series = (LineSeries) AltitudeChart.Series[0];
			series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(TimeService.UtcNow), e.Altitude));
			AltitudeChart.InvalidatePlot(true);
		}

		#endregion
	}
}