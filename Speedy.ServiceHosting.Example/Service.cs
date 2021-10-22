#region References

using System;
using System.Diagnostics.Tracing;

#endregion

namespace Speedy.ServiceHosting.Example
{
	public class Service : WindowsService<ServiceOptions>
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the WindowsService class.
		/// </summary>
		public Service(ServiceOptions options) : base(options)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// The thread for the service.
		/// </summary>
		protected override void Process()
		{
			var count = 0;

			while (IsRunning)
			{
				//CheckForUpdate();
				WriteLine($"Count: {count++}");
				Sleep(5000);
			}
		}

		protected override void WriteLine(string message, EventLevel level = EventLevel.Informational)
		{
			Console.WriteLine(message);
			base.WriteLine(message, level);
		}

		#endregion
	}
}