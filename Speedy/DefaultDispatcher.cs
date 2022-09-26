#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a default dispatcher
	/// </summary>
	public class DefaultDispatcher : IDispatcher
	{
		#region Properties

		/// <inheritdoc />
		public bool HasThreadAccess { get; private set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Run(Action action)
		{
			if (HasThreadAccess == false)
			{
				// Go ahead and mark thread access as true
				HasThreadAccess = true;
			}

			action();
		}

		/// <inheritdoc />
		public Task RunAsync(Action action)
		{
			if (HasThreadAccess == false)
			{
				// Go ahead and mark thread access as true
				HasThreadAccess = true;
			}

			action();
			return Task.CompletedTask;
		}

		#endregion
	}
}