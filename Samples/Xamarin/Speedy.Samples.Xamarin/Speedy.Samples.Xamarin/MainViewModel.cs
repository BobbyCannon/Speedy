#region References

using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Speedy.Application;
using Speedy.Application.Xamarin;
using Speedy.Collections;
using Speedy.Devices.Location;
using Speedy.Logging;

#endregion

namespace Speedy.Samples.Xamarin
{
	public class MainViewModel : ViewModel
	{
		#region Constructors

		public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
		{
			Logs = new LimitedObservableCollection<LogEventArgs>(100);
			LocationProvider = new XamarinLocationProvider<Location, LocationProviderSettingsView>(dispatcher);
			LocationProvider.LogEventWritten += LocationProviderOnLogEventWritten;
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

		public XamarinLocationProvider<Location, LocationProviderSettingsView> LocationProvider { get; }

		public ObservableCollection<LogEventArgs> Logs { get; }

		#endregion

		#region Methods

		private void LocationProviderOnLogEventWritten(object sender, LogEventArgs e)
		{
			Dispatcher.Run(() => Logs.Insert(0, e));
		}

		private void LocationProviderOnPositionChanged(object sender, Location e)
		{
			var series = (LineSeries) AltitudeChart.Series[0];
			series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(TimeService.UtcNow), e.Altitude));
			AltitudeChart.InvalidatePlot(true);
		}

		#endregion
	}
}