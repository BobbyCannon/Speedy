#region References

using Speedy.Data.Client;
using Speedy.Mobile.Services;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile.ViewModels
{
	public abstract class BaseViewModel : Bindable
	{
		#region Constructors

		protected BaseViewModel(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion

		#region Properties

		public IDataStore<ClientLogEvent> DataStore => DependencyService.Get<IDataStore<ClientLogEvent>>();

		public bool IsBusy { get; set; }

		public string Title { get; set; }

		#endregion
	}
}