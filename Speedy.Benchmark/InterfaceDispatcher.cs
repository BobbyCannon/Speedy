#region References

using System;
using System.Threading.Tasks;
using System.Windows.Threading;

#endregion

namespace Speedy.Benchmark
{
	public class InterfaceDispatcher : IDispatcher
	{
		#region Fields

		private readonly Dispatcher _dispatcher;

		#endregion

		#region Constructors

		public InterfaceDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		#endregion

		#region Properties

		public bool HasThreadAccess => _dispatcher.CheckAccess();

		#endregion

		#region Methods

		public void Run(Action action)
		{
			if (_dispatcher.CheckAccess())
			{
				action();
				return;
			}

			_dispatcher.Invoke(action, DispatcherPriority.Normal);
		}

		public Task RunAsync(Action action)
		{
			if (_dispatcher.CheckAccess())
			{
				action();
				return Task.CompletedTask;
			}

			return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
		}

		#endregion
	}
}