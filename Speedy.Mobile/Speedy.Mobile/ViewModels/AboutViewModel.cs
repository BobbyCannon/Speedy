﻿#region References

using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.ViewModels
{
	public class AboutViewModel : BaseViewModel
	{
		#region Constructors

		public AboutViewModel()
		{
			Title = "About";
			OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://xamarin.com"));
		}

		#endregion

		#region Properties

		public ICommand OpenWebCommand { get; }

		#endregion
	}
}