using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Speedy.Data.Client;
using Xamarin.Forms;

using Speedy.Mobile.Models;
using Speedy.Mobile.Views;

namespace Speedy.Mobile.ViewModels
{
	public class ItemsViewModel : BaseViewModel
	{
		public ObservableCollection<ClientLogEvent> Items { get; set; }
		public Command LoadItemsCommand { get; set; }

		public ItemsViewModel()
		{
			Title = "Browse";
			Items = new ObservableCollection<ClientLogEvent>();
			LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

			MessagingCenter.Subscribe<NewItemPage, ClientLogEvent>(this, "AddItem", async (obj, item) =>
			{
				var newItem = item as ClientLogEvent;
				Items.Add(newItem);
				await DataStore.AddItemAsync(newItem);
			});
		}

		async Task ExecuteLoadItemsCommand()
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
	}
}