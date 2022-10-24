#region References

using System;
using System.Threading.Tasks;
using Speedy.Extensions;
using XamarinForms = Xamarin.Forms;

#endregion

namespace Speedy.Application.Xamarin;

/// <inheritdoc />
public class XamarinDispatcher : Dispatcher
{
	#region Properties

	/// <inheritdoc />
	public override bool IsDispatcherThread => !XamarinForms.Device.IsInvokeRequired;

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void ExecuteOnDispatcher(Action action)
	{
		XamarinForms.Device.InvokeOnMainThreadAsync(action).AwaitResults();
	}

	/// <inheritdoc />
	protected override T ExecuteOnDispatcher<T>(Func<T> action)
	{
		return XamarinForms.Device.InvokeOnMainThreadAsync(action).AwaitResults();
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action)
	{
		return XamarinForms.Device.InvokeOnMainThreadAsync(action);
	}

	/// <inheritdoc />
	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action)
	{
		return XamarinForms.Device.InvokeOnMainThreadAsync(action);
	}

	#endregion
}