#region References

using System;
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
	public partial class ItemsPage
	{
		#region Fields

		private readonly ItemsViewModel _viewModel;

		#endregion

		#region Constructors

		public ItemsPage()
		{
			InitializeComponent();

			MobileDispatcher = new MobileDispatcher();
			BindingContext = _viewModel = new ItemsViewModel(MobileDispatcher);
		}

		public MobileDispatcher MobileDispatcher { get; }

		#endregion

		#region Methods

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (_viewModel.Items.Count == 0)
			{
				_viewModel.IsBusy = true;
			}
		}

		private async void AddItem_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new NavigationPage(new NewItemPage()));
		}

		private async void OnItemSelected(object sender, EventArgs args)
		{
			var layout = (BindableObject) sender;
			var item = (ClientLogEvent) layout.BindingContext;
			await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item, MobileDispatcher)));
		}

		#endregion
	}
}