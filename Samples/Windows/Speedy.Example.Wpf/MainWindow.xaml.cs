#region References

using System.ComponentModel;
using System.Windows;
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

		Loaded += OnLoaded;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion

	#region Methods

	protected override void OnClosing(CancelEventArgs e)
	{
		ViewModel.CancellationPending = true;
		ViewModel.WaitUntil(x => !x.IsRunning, 1000, 10);
		base.OnClosing(e);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
	}

	#endregion
}