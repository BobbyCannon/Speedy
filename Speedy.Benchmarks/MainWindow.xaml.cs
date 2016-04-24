#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy;
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
			Assert.AreEqual(server.Database.Addresses.Count(), client.Database.Addresses.Count());
			Assert.AreEqual(server.Database.People.Count(), client.Database.People.Count());
			Extensions.AreEqual(server.Database.Addresses, client.Database.Addresses);
			Extensions.AreEqual(server.Database.People, client.Database.People);
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
					ViewModel.SyncClients.Clear();
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Server", new ContosoDatabaseProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 1", new ContosoDatabaseProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 2", new ContosoDatabaseProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 3", new ContosoDatabaseProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 4", new ContosoDatabaseProvider())));
					ViewModel.SyncClients.Add(new SyncClientState(new ContosoSyncClient("Client 5", new ContosoDatabaseProvider())));
					Worker.RunWorkerAsync(ViewModel.SyncClients);
					break;

				default:
					Worker.CancelAsync();
					ViewModel.SyncClients.Clear();
					break;
			}

			ViewModel.SyncStatus = ViewModel.SyncStatus == "Sync" ? "Stop" : "Sync";
		}

		private bool UpdateClient(ContosoSyncClient client, bool forceAdd = false)
		{
			var number = Extensions.Random.Next(0, 100);
			var result = false;

			if (number % 5 == 0) // 20%
			{
				// Delete Person or Address?
				if (number > 50)
				{
					var person = client.Database.People.GetRandomItem();
					if (person != null)
					{
						Debug.WriteLine("Delete P: " + person.SyncId + " on " + client.Name);
						client.Database.People.Remove(person);
					}
				}
				else
				{
					var address = client.Database.Addresses.Where(x => !x.People.Any()).GetRandomItem();
					if (address != null)
					{
						Debug.WriteLine("Delete A: " + address.SyncId + " on " + client.Name);
						client.Database.Addresses.Remove(address);
					}
				}
			}

			if (number % 2 == 0) // 50%
			{
				// Change Person or Address?
				if (number > 50 && client.Database.People.Any())
				{
					var person = client.Database.People.GetRandomItem();
					person.Name = Extensions.LoremIpsumWord() + " " + Extensions.LoremIpsumWord();

					// 25% to change address.
					if (number % 4 == 0)
					{
						var address = client.Database.Addresses.GetRandomItem(person.Address);

						Debug.WriteLine("Updating P: " + person.SyncId + " to A: " + address.SyncId);
						person.Address = address;
					}
				}
			}

			if (number % 4 == 0 || forceAdd) // 25%
			{
				if (number > 50 && client.Database.Addresses.Any())
				{
					// Add Person?
					var address = client.Database.Addresses.GetRandomItem();
					client.Database.People.Add(new Person
					{
						Name = Extensions.LoremIpsumWord() + " " + Extensions.LoremIpsumWord(),
						AddressId = address.Id
					});
				}
				else
				{
					client.Database.Addresses.Add(new Address
					{
						City = Extensions.LoremIpsumWord(),
						Line1 = Extensions.Random.Next(0, 999) + " " + Extensions.LoremIpsumWord(),
						Line2 = string.Empty,
						Postal = Extensions.Random.Next(0, 999999).ToString("000000"),
						State = Extensions.LoremIpsumWord().Substring(0, 2)
					});
				}

				result = true;
			}

			client.SaveChanges();
			return result;
		}

		private void WorkerOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var timeout = DateTime.UtcNow;
			var collection = (ObservableCollection<SyncClientState>) args.Argument;

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

				WriteLine("Updating " + client.Client.Name);
				var forceAdd = UpdateClient(client.Client);

				WriteLine("Updating " + server.Client.Name);
				UpdateClient(server.Client, forceAdd);

				//Thread.Sleep(1000);

				var engine = new SyncEngine(client.Client, server.Client, client.LastSyncedOn);
				engine.SyncStatusChanged += (o, a) => worker.ReportProgress((int) a.Percent, a);
				engine.RunAsync();

				while (engine.Status != SyncEngineStatus.Stopped && !worker.CancellationPending)
				{
					Thread.Sleep(100);
				}

				engine.Stop();

				//Thread.Sleep(1000);

				try
				{
					CompareClients(client.Client, server.Client);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Boom: " + client.LastSyncedOn.TimeOfDay + "\r\n\r\n");
					Debug.WriteLine("Client");
					client.Client.Database.Addresses.OrderBy(x => x.SyncId).ForEach(x => Debug.WriteLine("A: " + x.SyncId + " : " + x.ModifiedOn.TimeOfDay));
					client.Client.Database.People.OrderBy(x => x.SyncId).ForEach(x => Debug.WriteLine("P: " + x.SyncId + " : " + x.ModifiedOn.TimeOfDay));
					Debug.WriteLine("\r\n\r\nServer");
					server.Client.Database.Addresses.OrderBy(x => x.SyncId).ForEach(x => Debug.WriteLine("A: " + x.SyncId + " : " + x.ModifiedOn.TimeOfDay));
					server.Client.Database.People.OrderBy(x => x.SyncId).ForEach(x => Debug.WriteLine("P: " + x.SyncId + " : " + x.ModifiedOn.TimeOfDay));

					WriteLine(ex.Message);
					worker.CancelAsync();
				}

				timeout = DateTime.UtcNow.AddMilliseconds(250);
			}
		}

		private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			var status = (SyncEngineStatusArgs) args.UserState;
			//WriteLine($"{status.Name}: {status.Count}/{status.Total} {args.ProgressPercentage} {status.Status}");

			var clientState = ViewModel.SyncClients.FirstOrDefault(x => x.Client.Name == status.Name);
			if (clientState == null)
			{
				return;
			}

			clientState.Status = status.Status;
			ViewModel.Progress = (int) status.Percent;

			if (clientState.Status == SyncEngineStatus.Stopped)
			{
				clientState.AddressCount = clientState.Client.Database.Addresses.Count();
				clientState.PeopleCount = clientState.Client.Database.People.Count();
				clientState.PreviousSyncedOn = clientState.LastSyncedOn;
				clientState.LastSyncedOn = DateTime.UtcNow;
				ViewModel.Progress = 0;
			}
		}

		private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			ViewModel.SyncStatus = "Sync";
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