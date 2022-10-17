#region References

using mDispatcher = Microsoft.Maui.Dispatching.IDispatcher;

#endregion

namespace Speedy.Application.Maui
{
	public class MauiDispatcher : IDispatcher
	{
		#region Fields

		private readonly mDispatcher _dispatcher;

		#endregion

		#region Constructors

		public MauiDispatcher(mDispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public bool HasThreadAccess => !_dispatcher.IsDispatchRequired;

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Run(Action action)
		{
			if (!HasThreadAccess)
			{
				_dispatcher.Dispatch(action);
				return;
			}

			action();
		}

		/// <inheritdoc />
		public Task RunAsync(Action action)
		{
			if (!HasThreadAccess)
			{
				return _dispatcher.DispatchAsync(action);
			}

			action();
			return Task.CompletedTask;
		}

		#endregion
	}
}