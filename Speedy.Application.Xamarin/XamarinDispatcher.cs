﻿#region References

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
	protected override void ExecuteOnDispatcher(Action action, DispatcherPriority priority)
	{
		using var task = XamarinForms.Device.InvokeOnMainThreadAsync(action);
		task.AwaitResults();
	}

	/// <inheritdoc />
	protected override T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority)
	{
		using var task = XamarinForms.Device.InvokeOnMainThreadAsync(action);
		var response = task.AwaitResults();
		return response;
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority)
	{
		return XamarinForms.Device.InvokeOnMainThreadAsync(action);
	}

	/// <inheritdoc />
	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority)
	{
		return XamarinForms.Device.InvokeOnMainThreadAsync(action);
	}

	#endregion
}