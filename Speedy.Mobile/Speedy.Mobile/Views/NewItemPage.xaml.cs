using System;
using System.Collections.Generic;
using System.ComponentModel;
using Speedy.Data.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Speedy.Mobile.Models;

namespace Speedy.Mobile.Views
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class NewItemPage : ContentPage
	{
		public ClientLogEvent Item { get; set; }

		public NewItemPage()
		{
			InitializeComponent();

			Item = new ClientLogEvent
			{
				Message = "Item name",
			};

			BindingContext = this;
		}

		async void Save_Clicked(object sender, EventArgs e)
		{
			MessagingCenter.Send(this, "AddItem", Item);
			await Navigation.PopModalAsync();
		}

		async void Cancel_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}