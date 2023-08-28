#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

#endregion

namespace Speedy.Benchmark
{
	public class MainViewModel : Bindable
	{
		#region Fields

		private BenchmarkResult _lastResult;

		#endregion

		#region Constructors

		public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
		{
			BenchmarkResults = new ObservableCollection<BenchmarkResult>();
			Worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			Worker.DoWork += MainViewWorker.DoWork;
			Worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
			Worker.ProgressChanged += WorkerOnProgressChanged;

			// Defaults
			AddressCount = 1;
			AccountCount = 1;
			CachePrimaryKeys = true;
			CreateData = true;
			IncrementSyncIds = true;
			ItemsPerSync = 600;
			UseBulkProcessing = true;
			UseKeyManagerForClient = true;
			UseKeyManagerForServer = true;
			UseVerboseLogging = true;
			Scenarios = new TestScenarios { SqlToSql = true };
			SyncData = true;

			// Setup commands
			StartWorkCommand = new RelayCommand(x => StartWork());
		}

		#endregion

		#region Properties

		public int AccountCount { get; set; }

		public int AddressCount { get; set; }

		public ObservableCollection<BenchmarkResult> BenchmarkResults { get; }

		public bool CachePrimaryKeys { get; set; }

		public bool CreateData { get; set; }

		public bool IncrementSyncIds { get; set; }

		public bool IsRunning => Worker.IsBusy;

		public int ItemsPerSync { get; set; }

		public DateTime LastSyncedOnClient { get; set; }

		public DateTime LastSyncedOnServer { get; set; }

		public string Log { get; private set; }

		public TestScenarios Scenarios { get; }

		public MainWindowSettings Settings { get; set; }

		public ICommand StartWorkCommand { get; }

		public bool SyncData { get; set; }

		public int TotalPercentage { get; private set; }

		public bool UseBulkProcessing { get; set; }

		public bool UseKeyManagerForClient { get; set; }

		public bool UseKeyManagerForServer { get; set; }

		public bool UseVerboseLogging { get; set; }

		public BackgroundWorker Worker { get; }

		#endregion

		#region Methods

		private void StartWork()
		{
			if (Worker.IsBusy)
			{
				if (Worker.CancellationPending)
				{
					return;
				}

				Worker.CancelAsync();
				OnPropertyChanged(nameof(IsRunning));
				return;
			}

			// Clear the results and start the worker
			BenchmarkResults.Clear();
			Log = string.Empty;
			Worker.RunWorkerAsync(new object[] { GetDispatcher(), this });
			OnPropertyChanged(nameof(IsRunning));
		}

		private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			switch (e.ProgressPercentage)
			{
				case (int) MainViewWorkerStatus.Log:
					Log += e.UserState + Environment.NewLine;
					break;

				case (int) MainViewWorkerStatus.AddResult:
					_lastResult = (BenchmarkResult) e.UserState;
					BenchmarkResults.Add(_lastResult);
					break;

				case (int) MainViewWorkerStatus.UpdateResultProgress:
					_lastResult.Percent = (double) e.UserState;
					break;

				case (int) MainViewWorkerStatus.StopResult:
					_lastResult.Percent = 100;
					_lastResult.Stop();
					break;

				default:
					TotalPercentage = e.ProgressPercentage;
					break;
			}
		}

		private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			TotalPercentage = 0;
			OnPropertyChanged(nameof(IsRunning));
		}

		#endregion
	}
}