#region References

using System;
using System.Threading.Tasks;
using Xamarin.Forms;

#endregion

namespace Speedy.Mobile
{
	public class MobileDispatcher : IDispatcher
	{
		#region Properties

		public bool HasThreadAccess => true;

		#endregion

		#region Methods

		public void Run(Action action)
		{
			if (!Device.IsInvokeRequired)
			{
				action();
				return;
			}

			Device.InvokeOnMainThreadAsync(action).Wait();
		}

		public Task RunAsync(Action action)
		{
			if (!Device.IsInvokeRequired)
			{
				action();
				return Task.CompletedTask;
			}

			return Device.InvokeOnMainThreadAsync(action);
		}

		#endregion
	}
}