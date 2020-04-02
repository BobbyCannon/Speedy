#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Speedy.Data.Client;
using Speedy.Mobile.Services;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		#region Fields

		private bool isBusy;

		private string title = string.Empty;

		#endregion

		#region Properties

		public IDataStore<ClientLogEvent> DataStore => DependencyService.Get<IDataStore<ClientLogEvent>>();

		public bool IsBusy
		{
			get => isBusy;
			set => SetProperty(ref isBusy, value);
		}

		public string Title
		{
			get => title;
			set => SetProperty(ref title, value);
		}

		#endregion

		#region Methods

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
			{
				return;
			}

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
			{
				return false;
			}

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		#endregion

		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}