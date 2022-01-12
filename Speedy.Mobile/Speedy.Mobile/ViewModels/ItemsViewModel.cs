#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Speedy.Client.Data;
using Speedy.Commands;
using Speedy.Data;
using Speedy.Data.Client;
using Speedy.EntityFramework;
using Speedy.Mobile.Views;
using Speedy.Net;
using Speedy.Profiling;
using Speedy.Sync;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Data.Sync;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.ViewModels
{
	public class ItemsViewModel : BaseViewModel
	{
		#region Fields

		private static readonly Expression<Func<ClientAccount, bool>> _accountRemove;
		private static readonly Expression<Func<ClientAddress, bool>> _addressRemove;
		private readonly DebounceService _debounceUpdate;
		private static readonly Expression<Func<ClientLogEvent, bool>> _logEventRemove;
		private int _progress;
		private static readonly Expression<Func<ClientSetting, bool>> _settingRemove;
		private readonly BackgroundWorker _worker;

		#endregion

		#region Constructors

		public ItemsViewModel() : base(new MobileDispatcher())
		{
		}

		public ItemsViewModel(IDispatcher dispatcher) : base(dispatcher)
		{
			_worker = new BackgroundWorker();
			_worker.WorkerReportsProgress = true;
			_worker.WorkerSupportsCancellation = true;
			_worker.DoWork += WorkerOnDoWork;
			_worker.ProgressChanged += WorkerOnProgressChanged;
			_worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;

			_debounceUpdate = new DebounceService(TimeSpan.FromSeconds(1), true);
			_debounceUpdate.Action += DebounceUpdateOnAction;

			Profiler = new ProfileService(new ProfilerRepository(), dispatcher);
			Profiler.Initialize();
			Profiler.Start();

			Title = "Browse";
			Items = new ObservableCollection<ClientLogEvent>();
			LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

			MessagingCenter.Subscribe<NewItemPage, ClientLogEvent>(this, "AddItem", async (obj, item) =>
			{
				var newItem = item;
				Items.Add(newItem);
				await DataStore.AddItemAsync(newItem);
			});

			// Commands
			StartSyncCommand = new RelayCommand(x => StartSync());
			StopSyncCommand = new RelayCommand(x => StopSync());
		}

		static ItemsViewModel()
		{
			_accountRemove = x => true;
			_addressRemove = x => true;
			_logEventRemove = x => true;
			_settingRemove = x => true;
		}

		#endregion

		#region Properties

		public ObservableCollection<ClientLogEvent> Items { get; set; }

		public Command LoadItemsCommand { get; set; }

		public ProfileService Profiler { get; }

		public ICommand StartSyncCommand { get; }

		public string Status { get; set; }

		public ICommand StopSyncCommand { get; }

		#endregion

		#region Methods

		public static void AddSaveAndCleanup<T, T2>(IContosoClientDatabase database, T item) where T : Entity<T2>
		{
			database.Add<T, T2>(item);
			database.SaveChanges();
			database.Dispose();

			// Because sync is based on "until" (less than, not equal) then we must delay at least a millisecond to delay the data.
			//Thread.Sleep(0);
		}

		public static IContosoClientDatabase GetClientDatabase(string connection, bool clear = true, DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
		{
			var database = ContosoClientDatabase.UseSqlite(connection, options, keyCache);

			if (clear)
			{
				database.Database.EnsureDeleted();
			}

			database.Database.Migrate();

			return database;
		}

		public static ISyncableDatabaseProvider<IContosoClientDatabase> GetClientDatabaseProvider(string connection, bool clear)
		{
			if (connection != null)
			{
				if (clear)
				{
					GetClientDatabase(connection, true).Dispose();
				}

				return new SyncableDatabaseProvider<IContosoClientDatabase>((o, c) => GetClientDatabase(connection, false, o, c), ContosoClientDatabase.GetDefaultOptions(), null);
			}

			var memoryDatabase = new ContosoClientMemoryDatabase();
			return new SyncableDatabaseProvider<IContosoClientDatabase>((o, c) =>
			{
				memoryDatabase.Options.UpdateWith(o);
				return memoryDatabase;
			}, ContosoClientDatabase.GetDefaultOptions(), null);
		}

		public static IContosoDatabase GetEntityDatabase(string connection, bool clear = true, DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
		{
			var database = ContosoSqliteDatabase.UseSqlite(connection, options, keyCache);

			if (clear)
			{
				database.Database.EnsureDeleted();
			}

			database.Database.Migrate();

			return database;
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetEntityDatabaseProvider(string connection, bool clear)
		{
			if (connection != null)
			{
				if (clear)
				{
					GetEntityDatabase(connection, true).Dispose();
				}

				return new SyncableDatabaseProvider<IContosoDatabase>((o, c) => GetEntityDatabase(connection, false, o, c), ContosoDatabase.GetDefaultOptions(), null);
			}

			var memoryDatabase = new ContosoMemoryDatabase();
			return new SyncableDatabaseProvider<IContosoDatabase>((o, c) =>
			{
				memoryDatabase.Options.UpdateWith(o);
				return memoryDatabase;
			}, ContosoClientDatabase.GetDefaultOptions(), null);
		}

		private void DebounceUpdateOnAction(object sender, EventArgs e)
		{
			Dispatcher.Run(() =>
			{
				Status = Profiler.RuntimeTimer.Elapsed.ToString();
				Debug.WriteLine(Status);
			});
		}

		private async Task ExecuteLoadItemsCommand()
		{
			IsBusy = true;

			try
			{
				Items.Clear();

				var items = await DataStore.GetItemsAsync(true);

				foreach (var item in items)
				{
					Items.Add(item);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private static void InitializeDatabase(ContosoClientMemoryDatabase database, DatabaseKeyCache keyCache)
		{
		}

		private void ManagerOnSyncCompleted(object sender, SyncResults<SyncType> e)
		{
			var manager = (ClientSyncManager) sender;
		}

		private ClientAddress NewAddress(string address)
		{
			return new ClientAddress
			{
				City = "City",
				CreatedOn = TimeService.UtcNow,
				Line1 = address,
				Line2 = string.Empty,
				ModifiedOn = TimeService.UtcNow,
				IsDeleted = false,
				Postal = "12345",
				State = "SC",
				SyncId = Guid.NewGuid()
			};
		}

		private ClientSetting NewSetting(string key, string value)
		{
			return new ClientSetting { Name = key, Value = value };
		}

		private void ResetDatabase(SyncClient client)
		{
			using var database = client.DatabaseProvider.GetDatabase();
			switch (database)
			{
				case EntityFrameworkDatabase ef:
				{
					ef.Database.EnsureDeleted();
					ef.Database.Migrate();
					break;
				}
				case ContosoClientMemoryDatabase memory:
				{
					memory.LogEvents.BulkRemove(_logEventRemove);
					memory.Settings.BulkRemove(_settingRemove);
					memory.Addresses.BulkRemove(_addressRemove);
					memory.Accounts.BulkRemove(_accountRemove);
					break;
				}
			}
		}

		private void RunSyncLoop(SyncOptions options, SyncClient client, SyncClient server)
		{
			try
			{
				//SyncProfile.Start();

				ResetDatabase(client);
				ResetDatabase(server);

				AddSaveAndCleanup<ClientAddress, long>(server.GetDatabase<IContosoClientDatabase>(), NewAddress("Blah1"));
				AddSaveAndCleanup<ClientAddress, long>(server.GetDatabase<IContosoClientDatabase>(), NewAddress("Blah2"));
				AddSaveAndCleanup<ClientAddress, long>(server.GetDatabase<IContosoClientDatabase>(), NewAddress("Blah3"));
				AddSaveAndCleanup<ClientSetting, long>(server.GetDatabase<IContosoClientDatabase>(), NewSetting("Foo1", "Bar1"));
				AddSaveAndCleanup<ClientSetting, long>(server.GetDatabase<IContosoClientDatabase>(), NewSetting("Foo2", "Bar2"));
				AddSaveAndCleanup<ClientSetting, long>(server.GetDatabase<IContosoClientDatabase>(), NewSetting("Foo3", "Bar3"));

				SyncEngine.Run(Guid.Empty, client, server, options).Dispose();

				Thread.Sleep(10);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void StartSync()
		{
			if (_worker.IsBusy)
			{
				return;
			}

			_worker.RunWorkerAsync();
		}

		private void StopSync()
		{
			_worker.CancelAsync();
		}

		private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
		{
			var worker = (BackgroundWorker) sender;

			Status = "Started";

			var path = ContosoClientDatabase.GetPersonalDataPath();
			//var options = new SyncOptions();
			//var client = new SyncClient("Client", GetClientDatabaseProvider($"Data Source={Path.Combine(path, "client.db")}", true));
			//var server = new SyncClient("Server", GetClientDatabaseProvider($"Data Source={Path.Combine(path, "server.db")}", true));
			var credential = new WebCredential("admin@speedy.local", "Password");
			var dispatcher = new DefaultDispatcher();

			var ipAddress = "10.0.0.3";
			var client = GetClientDatabaseProvider($"Data Source={Path.Combine(path, "client.db")}", true);
			var webClient = new WebClient($"https://{ipAddress}", 60000, credential, null, dispatcher);
			var webSyncClientProvider = new WebSyncClientProvider(webClient, new SyncableDatabaseProvider((o, c) => null, null, null));
			var manager = new ClientSyncManager(() => credential, client, webSyncClientProvider, Profiler, dispatcher);

			var server = GetEntityDatabaseProvider($"Data Source={Path.Combine(path, "server.db")}", true);
			//var server = GetEntityDatabaseProvider("server=10.0.0.3;database=Speedy;integrated security=true;TrustServerCertificate=True", true);
			var serverSyncClient = new ServerSyncClient(new AccountEntity(), server);
			var syncClientProvider = new SyncClientProvider(x => serverSyncClient);
			//var manager = new ClientSyncManager(() => credential, client, syncClientProvider, Profiler, dispatcher);
			manager.SyncCompleted += ManagerOnSyncCompleted;
			manager.Initialize();

			var watch = new Stopwatch();

			while (!worker.CancellationPending)
			{
				try
				{
					using var trackerPath = Profiler.StartNewPath("/test");
					using var d = client.GetDatabase();
					d.Addresses.Add(NewAddress($"Address {Profiler.AverageSyncTimeForAddresses.Count}"));
					d.SaveChanges();

					var result = manager.SyncAddresses();

					using var s = server.GetDatabase();
					Debug.WriteLine("Success: " + result.SyncSuccessful
						+ "Count: " + d.Addresses.Count()
						//+ " / " + s.Addresses.Count()
						+ " : " + result.Client.Statistics.AppliedChanges
						+ " : " + result.Server.Statistics.AppliedChanges
						+ " - " + result.SyncIssues.Count
						+ " - " + watch.Elapsed.TotalMilliseconds
					);

					watch.Restart();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}

				_progress++;

				worker.ReportProgress(_progress);
				Thread.Sleep(1);

				//if ((_progress % 100) == 0)
				//{
				//	GC.Collect();
				//}
			}
		}

		private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			_debounceUpdate.Trigger();
		}

		private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Status = "Stopped";
		}

		#endregion
	}
}