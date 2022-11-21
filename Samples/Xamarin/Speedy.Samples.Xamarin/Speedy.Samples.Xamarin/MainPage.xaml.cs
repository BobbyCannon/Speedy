#region References

using System;
using System.Linq;
using System.Text;
using Speedy.Collections;
using Speedy.Devices.Location;
using Speedy.Extensions;
using Speedy.Protocols.Csv;
using Speedy.Samples.Xamarin.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

#endregion

namespace Speedy.Samples.Xamarin;

public partial class MainPage
{
	#region Constructors

	public MainPage(MainViewModel mainViewModel)
	{
		InitializeComponent();

		ViewModel = mainViewModel;
		BindingContext = ViewModel;

		DeviceDisplay.KeepScreenOn = true;

		ViewModel.ExportHistoryRequest += ViewModelOnExportHistoryRequest;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion

	#region Methods

	private void ViewModelOnExportHistoryRequest(object sender, EventArgs e)
	{
		var now = TimeService.Now;

		foreach (var history in ViewModel.LocationHistory)
		{
			var data = CsvWriter.Write(history.Value.ToArray());
			if (data == null)
			{
				continue;
			}

			DependencyService
				.Get<IFileService>()
				.WriteFile($"{now:yy-MM-dd-hh-mm-ss}-location-export-{history.Key}.csv", data);
		}
	}

	#endregion
}