#region References

using System;
using Speedy.ServiceHosting;

#endregion

namespace Speedy.NmeaConsole
{
	public class ServiceOptions : WindowsServiceOptions
	{
		#region Constructors

		/// <summary>
		/// Creates an instance of the service options.
		/// </summary>
		public ServiceOptions() : base(Guid.Parse("E0755052-F83B-4561-9BA4-865132867328"), "Nmea.Console", "NMEA Console")
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// The baud rate to use. Defaults to 4800 if not provided.
		/// </summary>
		public int BaudRate => PropertyValue(nameof(BaudRate), 4800);

		/// <summary>
		/// The outgoing port.
		/// </summary>
		public int OutgoingPort => PropertyValue(nameof(OutgoingPort), -1);

		/// <summary>
		/// The serial port to open. If not provided the service will search for the GPS.
		/// </summary>
		public int SerialPort => PropertyValue(nameof(SerialPort), 0);

		/// <summary>
		/// Read timeout in milliseconds. Defaults to 30000 ms (aka 30 seconds).
		/// </summary>
		public int Timeout => PropertyValue(nameof(Timeout), 30000);

		#endregion

		#region Methods

		public override void SetupArguments()
		{
			Add(new WindowsServiceArgument { Help = "The baud rate of the serial port.", Name = "r", PropertyName = nameof(BaudRate) });
			Add(new WindowsServiceArgument { Help = "The port to use for Outgoing UDP packets.", Name = "o", PropertyName = nameof(OutgoingPort), IsRequired = true });
			Add(new WindowsServiceArgument { Help = "The number of the serial port. Ex. COM5 = 5", Name = "p", PropertyName = nameof(SerialPort) });
			Add(new WindowsServiceArgument { Help = "Read timeout in seconds", Name = "t", PropertyName = nameof(Timeout) });

			base.SetupArguments();
		}

		#endregion
	}
}