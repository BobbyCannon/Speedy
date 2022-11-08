#region References

using System;
using System.Linq;
using System.Text;
using Speedy.Devices.Location;
using Speedy.Extensions;
using Speedy.Samples.Xamarin.Services;
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

		ViewModel.ExportHistoryRequest += ViewModelOnExportHistoryRequest;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion

	#region Methods

	private void ViewModelOnExportHistoryRequest(object sender, EventArgs e)
	{
		var properties = typeof(Location)
			.GetCachedProperties()
			.OrderBy(x => x.Name)
			.ToList();

		var now = TimeService.Now;

		foreach (var history in ViewModel.LocationHistory)
		{
			var builder = new StringBuilder();
			builder.AppendLine(string.Join(",", properties.Select(x => x.Name)));
			
			foreach (var x in history.Value)
			{
				var values = properties.Select(p => p.GetValue(x).ToString()).ToList();
				builder.AppendLine(string.Join(",", values));
			}
			
			DependencyService
				.Get<IFileService>()
				.WriteFile($"{now:yy-MM-dd-hh-mm-ss}-location-export-{history.Key}.csv", builder.ToString());
		}
	}

	#endregion

	
}