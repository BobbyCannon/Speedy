#region References

using System.ComponentModel;
using Speedy.Data.Client;
using Speedy.Mobile.ViewModels;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.Views
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class ItemDetailPage : ContentPage
	{
		#region Fields

		private readonly ItemDetailViewModel viewModel;

		#endregion

		#region Constructors

		public ItemDetailPage(ItemDetailViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = this.viewModel = viewModel;
		}

		public ItemDetailPage()
		{
			InitializeComponent();

			var item = new ClientLogEvent
			{
				Message = "Item 1"
			};

			viewModel = new ItemDetailViewModel(item, new MobileDispatcher());
			BindingContext = viewModel;
		}

		#endregion
	}
}