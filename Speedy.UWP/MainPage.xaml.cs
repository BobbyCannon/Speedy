#region References

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.EntityFrameworkCore;
using Speedy.Client.Data;

#endregion

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Speedy.UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		#region Constructors

		public MainPage()
		{
			InitializeComponent();

			DefaultSqliteConnection = "Data Source=Speedy.db";

			Loaded += OnLoaded;
		}

		#endregion

		#region Properties

		public string DefaultSqliteConnection { get; }

		#endregion

		#region Methods

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			using (var database = ContosoClientDatabase.UseSqlite(DefaultSqliteConnection))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();

				Status.Text = "Database Migrated";
			}
		}

		#endregion
	}
}