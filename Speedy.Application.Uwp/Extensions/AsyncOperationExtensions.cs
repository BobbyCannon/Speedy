#region References

using System;
using Windows.Foundation;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp.Extensions;

public static class AsyncOperationExtensions
{
	#region Methods

	public static void AwaitResults(this IAsyncAction operation)
	{
		operation.AsTask().AwaitResults();
	}

	public static T AwaitResults<T>(this IAsyncOperation<T> operation)
	{
		return operation.AsTask().AwaitResults();
	}

	public static T AwaitResults<T>(this IAsyncOperation<T> operation, TimeSpan timeout)
	{
		return operation.AsTask().AwaitResults(timeout);
	}

	#endregion
}