#region References

using System;
using Speedy.ServiceHosting;

#endregion

namespace Speedy.SerialConsole
{
	public class ServiceOptions : WindowsServiceOptions
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of the service options.
		/// </summary>
		public ServiceOptions() : base(Guid.Parse("4C6CE173-5485-4755-87C8-A647E573DA08"), "Serial.Console", "Serial Console")
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// The baud rate to use. Defaults to 9600 if not provided.
		/// </summary>
		public int BaudRate => PropertyValue<int>(nameof(BaudRate));

		/// <summary>
		/// The network address. Defaults to 127.0.0.1.
		/// </summary>
		public string NetworkAddress => PropertyValue<string>(nameof(NetworkAddress));

		/// <summary>
		/// The network port. Defaults to 32000.
		/// </summary>
		public int NetworkPort => PropertyValue<int>(nameof(NetworkPort));

		/// <summary>
		/// The serial port to open. If not provided the service will search for the GPS.
		/// </summary>
		public int SerialPort => PropertyValue<int>(nameof(SerialPort));

		/// <summary>
		/// Read timeout in milliseconds. Defaults to 30000 ms (aka 30 seconds).
		/// </summary>
		public int Timeout => PropertyValue<int>(nameof(Timeout));

		#endregion

		#region Methods

		public override void SetupArguments()
		{
			Add(new WindowsServiceArgument<int> { Help = "The baud rate of the serial port.", Name = "r", PropertyName = nameof(BaudRate), DefaultValue = 9600 });
			Add(new WindowsServiceArgument<string> { Help = "The IP to use for incoming TCP connection.", Name = "a", PropertyName = nameof(NetworkAddress), DefaultValue = "127.0.0.1" });
			Add(new WindowsServiceArgument<int> { Help = "The port to use for incoming TCP connection.", Name = "n", PropertyName = nameof(NetworkPort), DefaultValue = 32000 });
			Add(new WindowsServiceArgument<int> { Help = "The number of the serial port. Ex. COM5 = 5", Name = "p", PropertyName = nameof(SerialPort), IsRequired = true });
			Add(new WindowsServiceArgument<int> { Help = "Read timeout in seconds", Name = "t", PropertyName = nameof(Timeout), DefaultValue = 30000 });

			base.SetupArguments();
		}

		#endregion
	}
}