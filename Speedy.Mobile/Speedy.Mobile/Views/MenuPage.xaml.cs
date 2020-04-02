#region References

using System.Collections.Generic;
using System.ComponentModel;
using Speedy.Mobile.Models;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.Views
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class MenuPage : ContentPage
	{
		#region Fields

		private readonly List<HomeMenuItem> menuItems;

		#endregion

		#region Constructors

		public MenuPage()
		{
			InitializeComponent();

			menuItems = new List<HomeMenuItem>
			{
				new HomeMenuItem { Id = MenuItemType.Browse, Title = "Browse" },
				new HomeMenuItem { Id = MenuItemType.About, Title = "About" }
			};

			ListViewMenu.ItemsSource = menuItems;

			ListViewMenu.SelectedItem = menuItems[0];
			ListViewMenu.ItemSelected += async (sender, e) =>
			{
				if (e.SelectedItem == null)
				{
					return;
				}

				var id = (int) ((HomeMenuItem) e.SelectedItem).Id;
				await RootPage.NavigateFromMenu(id);
			};
		}

		#endregion

		#region Properties

		private MainPage RootPage => Application.Current.MainPage as MainPage;

		#endregion
	}
}