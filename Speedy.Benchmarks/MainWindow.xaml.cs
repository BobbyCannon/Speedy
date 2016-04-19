#region References

using System;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		}

		#endregion

		#region Methods

		private Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		private void SyncOnClick(object sender, RoutedEventArgs e)
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

				var client = new SyncClient();
				var server = new SyncServerClient("http://localhost");

				client.Addresses.Add(NewAddress("Blah"));
				client.SaveChanges();
				
				SyncEngine.PullAndPushChanges(client, server);
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

		private void WriteLine(string message)
		{
			Output.Text += message + Environment.NewLine;
		}

		#endregion
	}
}