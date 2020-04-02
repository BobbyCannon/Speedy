using System;
using Speedy.Data.Client;
using Speedy.Mobile.Models;

namespace Speedy.Mobile.ViewModels
{
	public class ItemDetailViewModel : BaseViewModel
	{
		public ClientLogEvent Item { get; set; }
		public ItemDetailViewModel(ClientLogEvent item = null)
		{
			Title = item?.Message;
			Item = item;
		}
	}
}
