#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a thread dispatcher to help with cross threaded request.
	/// </summary>
	public interface IDispatcher
	{
		#region Properties

		/// <summary>
		/// Has access to the dispatching thread.
		/// </summary>
		bool HasThreadAccess { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Run an action on the dispatching thread.
		/// </summary>
		/// <param name="action"> </param>
		void Run(Action action);

		/// <summary>
		/// Run an asynchronous action on the dispatching thread.
		/// </summary>
		/// <param name="action"> </param>
		/// <returns> </returns>
		Task RunAsync(Action action);

		#endregion
	}
}