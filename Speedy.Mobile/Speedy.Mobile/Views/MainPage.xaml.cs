#region References

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Speedy.Mobile.Models;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.Views
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class MainPage
	{
		#region Fields

		private readonly Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();

		#endregion

		#region Constructors

		public MainPage()
		{
			InitializeComponent();

			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

			MenuPages.Add((int) MenuItemType.Browse, (NavigationPage) Detail);
		}

		#endregion

		#region Methods

		public async Task NavigateFromMenu(int id)
		{
			if (!MenuPages.ContainsKey(id))
			{
				switch (id)
				{
					case (int) MenuItemType.Browse:
					{
						MenuPages.Add(id, new NavigationPage(new ItemsPage()));
						break;
					}
					case (int) MenuItemType.About:
					{
						MenuPages.Add(id, new NavigationPage(new AboutPage()));
						break;
					}
				}
			}

			var newPage = MenuPages[id];

			if ((newPage != null) && (Detail != newPage))
			{
				Detail = newPage;

				if (Device.RuntimePlatform == Device.Android)
				{
					await Task.Delay(100);
				}

				IsPresented = false;
			}
		}

		#endregion
	}
}