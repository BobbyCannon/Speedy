#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			Worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
			Worker.ProgressChanged += WorkerOnProgressChanged;
		}

		#endregion

		#region Properties

		public MainWindowModel ViewModel { get; }

		public BackgroundWorker Worker { get; set; }

		#endregion

		#region Methods

		private void Clear()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(Clear);
				return;
			}

			ViewModel.Output = string.Empty;
		}

		private static void CompareClients(IContosoSyncClient client, IContosoSyncClient server)
		{
			using (var clientDatabase = client.GetDatabase())
			using (var serverDatabase = server.GetDatabase())
			{
				Assert.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
				Assert.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
				// Fails when comparing two EF context?
				//Extensions.AreEqual(serverDatabase.Addresses, clientDatabase.Addresses);
				//Extensions.AreEqual(serverDatabase.People, clientDatabase.People);
			}
		}

		private void MainWindowOnClosing(object sender, CancelEventArgs e)
		{
			Worker.CancelAsync();
		}

		private void SyncOnClick(object sender, RoutedEventArgs e)
		{
			switch (ViewModel.SyncStatus)
			{
				case "Sync":
					ViewModel.Errors = string.Empty;
					ViewModel.SyncClients.Clear();
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Server", GetEntityFrameworkProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 1", GetEntityFrameworkProvider("server=localhost;database=Speedy2;integrated security=true;"))));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 2", new ContosoDatabaseProvider())));
					// Not valid as long as the progress report has to open the database again...
					//ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 3", new ContosoDatabaseProvider(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Speedy"))));
					Worker.RunWorkerAsync(ViewModel.SyncClients);
					break;

				default:
					Worker.CancelAsync();
					ViewModel.SyncClients.Clear();
					break;
			}

			ViewModel.SyncStatus = ViewModel.SyncStatus == "Sync" ? "Stop" : "Sync";
		}

		private static IContosoDatabaseProvider GetEntityFrameworkProvider(string connectionString = null)
		{
			using (var database = new EntityFrameworkContosoDatabase(connectionString ?? "name=DefaultConnection"))
			{
				database.ClearDatabase();
				return new EntityFrameworkContosoDatabaseProvider(database.Database.Connection.ConnectionString);
			}
		}

		private static bool UpdateClient(IContosoSyncClient client, bool forceAdd = false)
		{
			var number = Extensions.Random.Next(1, 101);
			var result = false;

			if (number % 4 == 0 || forceAdd) // 25%
			{
				using (var clientDatabase = client.GetDatabase())
				{
					if (number > 50 && clientDatabase.Addresses.Any())
					{
						// Add Person?
						var address = clientDatabase.Addresses.GetRandomItem();
						clientDatabase.People.Add(new Person
						{
							Name = Extensions.LoremIpsumWord() + " " + Extensions.LoremIpsumWord(),
							AddressId = address.Id
						});
					}
					else
					{
						clientDatabase.Addresses.Add(new Address
						{
							City = Extensions.LoremIpsumWord(),
							Line1 = Extensions.Random.Next(0, 999) + " " + Extensions.LoremIpsumWord(),
							Line2 = string.Empty,
							Postal = Extensions.Random.Next(0, 999999).ToString("000000"),
							State = Extensions.LoremIpsumWord().Substring(0, 2)
						});
					}

					result = true;
					clientDatabase.SaveChanges();
				}
			}

			if (number % 10 == 0) // 10%
			{
				using (var clientDatabase = client.GetDatabase())
				{
					// Delete Person or Address?
					if (number > 50)
					{
						var person = clientDatabase.People.GetRandomItem();
						if (person != null)
						{
							clientDatabase.People.Remove(person);
						}
					}
					else
					{
						var address = clientDatabase.Addresses.Where(x => !x.People.Any()).GetRandomItem();
						if (address != null)
						{
							clientDatabase.Addresses.Remove(address);
						}
					}

					clientDatabase.SaveChanges();
				}
			}

			return result;
		}

		private void WorkerOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var timeout = DateTime.UtcNow;
			var collection = (ObservableCollection<SyncClientState>) args.Argument;
			var options = new SyncOptions();
			WriteError("Worker Started");

			while (!worker.CancellationPending)
			{
				if (timeout > DateTime.UtcNow)
				{
					Thread.Sleep(100);
					continue;
				}

				var server = collection[0];
				var client = collection.GetRandomItem(server);

				Clear();

				try
				{
					WriteLine("Updating " + client.Client.Name);
					var forceAdd = UpdateClient(client.Client);

					WriteLine("Updating " + server.Client.Name);
					UpdateClient(server.Client, forceAdd);
				}
				catch (Exception ex)
				{
					// Write message but ignore them for now...
					WriteError(ex.Message);
				}

				//Thread.Sleep(300);

				options.LastSyncedOn = client.LastSyncedOn;
				var engine = new SyncEngine(client.Client, server.Client, options);
				engine.SyncStatusChanged += (o, a) => worker.ReportProgress((int) a.Percent, a);
				engine.Run();

				foreach (var item in engine.SyncIssues)
				{
					WriteError(item.Id + " - " + item.IssueType + " : " + item.TypeName);
				}

				//Thread.Sleep(300);

				try
				{
					CompareClients(client.Client, server.Client);
				}
				catch (Exception ex)
				{
					WriteError("StartTime: " + engine.StartTime);
					WriteError("LastSyncedOn: " + options.LastSyncedOn);
					WriteError(ex?.Message ?? "null?");
					worker.CancelAsync();
				}

				timeout = DateTime.UtcNow.AddMilliseconds(250);
			}

			WriteError("Worker Ending");
		}

		private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			var status = (SyncEngineStatusArgs) args.UserState;
			var clientState = ViewModel.SyncClients.FirstOrDefault(x => x.Client.Name == status.Name);
			if (clientState == null)
			{
				return;
			}

			clientState.Status = status.Status;
			ViewModel.Progress = (int) status.Percent;

			if (clientState.Status == SyncEngineStatus.Stopped)
			{
				using (var database = clientState.Client.GetDatabase())
				{
					clientState.AddressCount = database.Addresses.Count();
					clientState.PeopleCount = database.People.Count();
					clientState.PreviousSyncedOn = clientState.LastSyncedOn;
					clientState.LastSyncedOn = DateTime.UtcNow;
					ViewModel.Progress = 0;
				}
			}
		}

		private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			ViewModel.SyncStatus = "Sync";
		}

		private void WriteError(string message)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => WriteError(message));
				return;
			}

			ViewModel.Errors += message + Environment.NewLine;
		}

		private void WriteLine(string message)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => WriteLine(message));
				return;
			}

			ViewModel.Output += message + Environment.NewLine;
		}

		#endregion
	}
}