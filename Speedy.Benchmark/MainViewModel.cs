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
			CreateData = true;
			SyncData = true;
			UseBulkProcessing = true;
			CachePrimaryKeys = false;
			AddressCount = 1;
			AccountCount = 5000;

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

		public string Log { get; private set; }

		public ICommand StartWorkCommand { get; }

		public bool SyncData { get; set; }

		public int TotalPercentage { get; private set; }

		public bool UseBulkProcessing { get; set; }

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
				return;
			}

			// Clear the results and start the worker
			BenchmarkResults.Clear();
			Log = string.Empty;
			Worker.RunWorkerAsync(new object[] { Dispatcher, this });
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
		}

		#endregion
	}
}