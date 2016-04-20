#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Samples.Sync;
using Speedy.Sync;

#endregion

namespace Speed.Benchmarks
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
			ViewModel = new MainWindowModel();
			DataContext = ViewModel;

			Worker = new BackgroundWorker();
			Worker.WorkerReportsProgress = true;
			Worker.WorkerSupportsCancellation = true;
			Worker.DoWork += WorkerOnDoWork;
			Worker.ProgressChanged += WorkerOnProgressChanged;
		}

		#endregion

		#region Properties

		public MainWindowModel ViewModel { get; }

		public BackgroundWorker Worker { get; set; }

		#endregion

		#region Methods

		private static Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		private void SyncOnClick(object sender, RoutedEventArgs e)
		{
			switch (ViewModel.SyncStatus)
			{
				case "Sync":
					ViewModel.SyncClients.Add(new ContosoDatabaseSyncClient("Client1", new ContosoDatabase()));
					ViewModel.SyncClients.Add(new ContosoDatabaseSyncClient("Client2", new ContosoDatabase()));
					ViewModel.SyncClients.Add(new ContosoDatabaseSyncClient("Client3", new ContosoDatabase()));
					Worker.RunWorkerAsync(ViewModel.SyncClients);
					break;

				default:
					Worker.CancelAsync();
					ViewModel.SyncClients.Clear();
					break;
			}

			ViewModel.SyncStatus = ViewModel.SyncStatus == "Sync" ? "Stop" : "Sync";
		}

		private void SyncOnClick2(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var database = new EntityFrameworkContosoDatabase())
				{
					database.ClearDatabase();

					var address = NewAddress("Foo");
					var person = new Person { Address = address, Name = "John Smith" };

					database.People.Add(person);
					database.SaveChanges();
				}

				var client = new ContosoDatabaseSyncClient("EF", new EntityFrameworkContosoDatabase());
				var server = new WebSyncClient("http://localhost");

				client.Addresses.Add(NewAddress("Blah"));
				client.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();
				client.SaveChanges();

				using (var serverDatabase = new EntityFrameworkContosoDatabase())
				{
					WriteLine($"{client.Addresses.Count()} Client Addresses");
					WriteLine($"{serverDatabase.Addresses.Count()} Server Addresses");

					var failed = client.Addresses.Count() != 1 || serverDatabase.Addresses.Count() != 1;
					var message = failed ? "failed" : "succeeded";

					WriteLine($"Sync {message}!");
				}
			}
			catch (Exception ex)
			{
				WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
			}
		}

		private static void WorkerOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var timeout = DateTime.UtcNow;
			var collection = (ObservableCollection<ISyncClient>) args.Argument;

			while (!worker.CancellationPending)
			{
				if (timeout < DateTime.UtcNow)
				{
					Thread.Sleep(100);
					continue;
				}

				var first = collection.GetRandomItem();
				var next = collection.GetRandomItem(first);

				//SyncEngine.PullAndPushChanges(first, next);
			}
		}

		private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			
		}

		private void WriteLine(string message)
		{
			ViewModel.Output += message + Environment.NewLine;
		}

		#endregion
	}
}