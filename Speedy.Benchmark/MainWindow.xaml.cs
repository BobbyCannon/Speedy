#region References

using System.Windows.Controls;
using Speedy.Profiling;

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

			Profiler.Enabled = true;

			var dispatcher = new InterfaceDispatcher(Dispatcher);
			ViewModel = new MainViewModel(dispatcher);
			DataContext = ViewModel;

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

		#endregion
	}
}