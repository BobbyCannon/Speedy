#region References

using System;
using System.ComponentModel;
using Speedy.Data.Client;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.Views
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class NewItemPage : ContentPage
	{
		#region Constructors

		public NewItemPage()
		{
			InitializeComponent();

			Item = new ClientLogEvent
			{
				Message = "Item name"
			};

			BindingContext = this;
		}

		#endregion

		#region Properties

		public ClientLogEvent Item { get; set; }

		#endregion

		#region Methods

		private async void Cancel_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}

		private async void Save_Clicked(object sender, EventArgs e)
		{
			MessagingCenter.Send(this, "AddItem", Item);
			await Navigation.PopModalAsync();
		}

		#endregion
	}
}