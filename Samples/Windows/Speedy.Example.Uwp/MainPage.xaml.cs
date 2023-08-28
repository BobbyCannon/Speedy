#region References

using Windows.UI.Core.Preview;
using Windows.UI.Xaml.Navigation;
using Speedy.Application.Uwp;
using Speedy.Example.Core;
using Speedy.Extensions;

#endregion

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Speedy.Uwp.Example;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage
{
	#region Fields

	private bool _navigatedTo;

	#endregion

	#region Constructors

	public MainPage()
	{
		InitializeComponent();

		var dispatcher = new UwpDispatcher(Dispatcher);
		var runtimeInformation = new UwpRuntimeInformation(dispatcher);

		ViewModel = new MainViewModel(runtimeInformation, dispatcher);
		DataContext = ViewModel;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion

	#region Methods

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		if (!_navigatedTo)
		{
			_navigatedTo = true;
			SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += AppOnCloseRequested;
		}

		base.OnNavigatedTo(e);
	}

	private void AppOnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
	{
		ViewModel.CancellationPending = true;
		UtilityExtensions.WaitUntil(() => !ViewModel.IsRunning, 1000, 10);
	}

	#endregion
}