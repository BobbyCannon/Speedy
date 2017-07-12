#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Samples.EntityFramework;

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

			DatabaseWorkers = new List<BackgroundWorker>();
			
			RepositoryWorker = new BackgroundWorker();
			RepositoryWorker.WorkerReportsProgress = true;
			RepositoryWorker.WorkerSupportsCancellation = true;
			RepositoryWorker.DoWork += RepositoryWorkerOnDoWork;
			RepositoryWorker.RunWorkerCompleted += RepositoryWorkerOnRunWorkerCompleted;
			RepositoryWorker.ProgressChanged += RepositoryWorkerOnProgressChanged;
		}

		#endregion

		#region Properties

		public List<BackgroundWorker> DatabaseWorkers { get; }

		public BackgroundWorker RepositoryWorker { get; }

		public MainWindowModel ViewModel { get; }

		#endregion

		#region Methods

		private static void AddToDatabase(IContosoDatabase clientDatabase)
		{
			Address address;
			var number = Extensions.Random.Next(0, 101);

			if (number % 2 == 0 && clientDatabase.Addresses.Any())
			{
				address = clientDatabase.Addresses.GetRandomItem();
				if (address == null)
				{
					return;
				}

				var person = new Person
				{
					Name = Extensions.LoremIpsumWord() + " " + Extensions.LoremIpsumWord(),
					AddressId = address.Id
				};

				clientDatabase.People.Add(person);
			}
			else
			{
				address = new Address
				{
					City = Extensions.LoremIpsumWord(),
					Line1 = Extensions.Random.Next(0, 999) + " " + Extensions.LoremIpsumWord(),
					Line2 = string.Empty,
					Postal = Extensions.Random.Next(0, 999999).ToString("000000"),
					State = Extensions.LoremIpsumWord().Substring(0, 2)
				};

				clientDatabase.Addresses.Add(address);
			}
		}
	
		
		private void DatabaseOnClick(object sender, RoutedEventArgs e)
		{
			switch (ViewModel.DatabaseStatus)
			{
				case "Database":
					new DirectoryInfo("C:\\Users\\Bobby\\Desktop\\Contoso").SafeDelete();

					for (var i = 0; i < 4; i++)
					{
						var worker = new BackgroundWorker();
						var client = new DatabaseClient();
						worker.WorkerSupportsCancellation = true;
						worker.DoWork += DatabaseWorkerDoWork;
						worker.RunWorkerCompleted += DatabaseWorkerCompleted;
						worker.RunWorkerAsync(client);
						ViewModel.DatabaseClients.Add(client);
						DatabaseWorkers.Add(worker);
					}

					ViewModel.DatabaseStatus = "Stop";
					break;

				default:
					DatabaseWorkers.ForEach(x => x.CancelAsync());
					ViewModel.DatabaseStatus = "Database";
					break;
			}
		}

		private void DatabaseWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ViewModel.DatabaseClients.Remove((DatabaseClient) e.Result);
			DatabaseWorkers.Remove((BackgroundWorker) sender);
		}

		private void DatabaseWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			var client = (DatabaseClient) e.Argument;
			var worker = (BackgroundWorker) sender;
			var threadId = Thread.CurrentThread.ManagedThreadId;

			using (var database = new ContosoMemoryDatabase("C:\\Users\\Bobby\\Desktop\\Contoso"))
			{
				while (!worker.CancellationPending)
				{
					try
					{
						AddToDatabase(database);
						database.SaveChanges();
					}
					catch
					{
						database.DiscardChanges();
						continue;
					}

					var addressCount = database.Addresses.Count();
					var peopleCount = database.People.Count();

					Dispatcher.Invoke(() =>
					{
						client.Name = "Thread: " + threadId;
						client.LastProcessedOn = DateTime.Now;
						client.AddressCount = addressCount;
						client.PeopleCount = peopleCount;
					});

					Thread.Sleep(Extensions.Random.Next(10, 100));
				}
			}

			e.Result = client;
		}

		private void MainWindowOnClosing(object sender, CancelEventArgs e)
		{
		}

		private void RepositoryOnClick(object sender, RoutedEventArgs e)
		{
			switch (ViewModel.RepositoryStatus)
			{
				case "Repository":
					RepositoryWorker.RunWorkerAsync();
					ViewModel.RepositoryStatus = "Stop";
					break;

				default:
					RepositoryWorker.CancelAsync();
					break;
			}
		}

		private void RepositoryWorkerOnDoWork(object sender, DoWorkEventArgs e)
		{
		}

		private void RepositoryWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
		{
		}

		private void RepositoryWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			ViewModel.RepositoryStatus = "Repository";
		}

		#endregion
	}
}