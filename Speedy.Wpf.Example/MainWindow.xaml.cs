#region References

using Speedy.Application.Wpf;

#endregion

namespace Speedy.Wpf.Example;

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
		ViewModel = new MainViewModel(dispatcher);
		DataContext = ViewModel;
	}

	#endregion

	#region Properties

	public MainViewModel ViewModel { get; }

	#endregion
}