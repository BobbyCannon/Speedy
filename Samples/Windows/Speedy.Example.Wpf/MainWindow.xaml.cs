#region References

using System.ComponentModel;
using Speedy.Application.Wpf;
using Speedy.Example.Core;
using Speedy.Extensions;

#endregion

namespace Speedy.Example.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	#region Constructors

	public MainWindow()
	{
		InitializeComponent();

		var dispatcher = new WpfDispatcher(Dispatcher);
		var runtimeInformation = new WpfRuntimeInformation(dispatcher);

		ViewModel = new MainViewModel(runtimeInformation, dispatcher);
		DataContext = ViewModel;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion

	#region Methods

	protected override void OnClosing(CancelEventArgs e)
	{
		ViewModel.CancellationPending = true;
		UtilityExtensions.WaitUntil(() => !ViewModel.IsRunning, 1000, 10);
		base.OnClosing(e);
	}

	#endregion
}