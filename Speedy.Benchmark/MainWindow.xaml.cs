#region References

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Speedy.Benchmark
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Constructors

		public MainWindow()
		{
			InitializeComponent();

			var dispatcher = new InterfaceDispatcher(Dispatcher);
			ViewModel = new MainViewModel(dispatcher);
			DataContext = ViewModel;

			Loaded += OnLoaded;
			Closing += OnClosing;
			Log.TextChanged += LogOnTextChanged;
		}

		#endregion

		#region Properties

		public MainViewModel ViewModel { get; }

		#endregion

		#region Methods

		private void LogOnTextChanged(object sender, TextChangedEventArgs e)
		{
			Log.ScrollToEnd();
		}

		private void OnClosing(object sender, CancelEventArgs e)
		{
			ViewModel.Settings.Top = (int) Top;
			ViewModel.Settings.Left = (int) Left;
			ViewModel.Settings.Height = (int) Height;
			ViewModel.Settings.Width = (int) Width;
			ViewModel.Settings.Save();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			ViewModel.Settings = MainWindowSettings.Load();

			if (ViewModel.Settings.Loaded)
			{
				Top = ViewModel.Settings.Top;
				Left = ViewModel.Settings.Left;
				Height = ViewModel.Settings.Height;
				Width = ViewModel.Settings.Width;
			}
		}

		#endregion
	}
}