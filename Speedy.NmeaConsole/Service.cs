#region References

using System;
using System.Diagnostics.Tracing;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Speedy.Protocols.Nmea;
using Speedy.ServiceHosting;

#endregion

namespace Speedy.NmeaConsole
{
	public class Service : WindowsService<ServiceOptions>
	{
		#region Fields

		private UdpClient _network;
		private IPEndPoint _networkEndPoint;
		private readonly NmeaParser _parser;

		#endregion

		#region Constructors

		public Service(ServiceOptions options) : base(options)
		{
			_parser = new NmeaParser();
		}

		#endregion

		#region Methods

		protected override void Process()
		{
			SerialPort port = null;

			_network = new UdpClient { ExclusiveAddressUse = false };
			_network.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_network.Client.Bind(new IPEndPoint(IPAddress.Any, Options.OutgoingPort));
			_networkEndPoint = new IPEndPoint(IPAddress.Broadcast, Options.OutgoingPort);

			if (Options.SerialPort > 0)
			{
				try
				{
					port = new SerialPort($"COM{Options.SerialPort}", Options.BaudRate, Parity.None, 8, StopBits.One);
					WriteLine($"{port.PortName} : {port.BaudRate}");
					port.Open();
				}
				catch
				{
					port?.Dispose();
					port = null;
				}
			}
			else
			{
				port = FindGpsDevice();
			}

			if (port == null)
			{
				WriteLine("Failed to find the GPS device.");
			}
			else
			{
				port.ReadTimeout = Options.Timeout;

				WriteLine($"Found GPS device on port {port.PortName}:{port.BaudRate}, Read Timeout: {port.ReadTimeout}");
				port.DataReceived += PortOnDataReceived;
			}
		}

		protected override void WriteLine(string message, EventLevel level = EventLevel.Informational)
		{
			Console.WriteLine(message);
			base.WriteLine(message, level);
		}

		private SerialPort FindGpsDevice()
		{
			var rates = new[] { 4800, 9600 };

			for (var i = 1; i <= 255; i++)
			{
				var port = new SerialPort($"COM{i}", 4800, Parity.None, 8, StopBits.One);

				try
				{
					foreach (var rate in rates)
					{
						port.BaudRate = rate;
						port.Open();
						port.ReadTimeout = 10000;

						WriteLine($"{port.PortName} : {port.BaudRate}");

						for (var j = 0; j < 5; j++)
						{
							try
							{
								var line = port.ReadLine();
								var message = _parser.Parse(line, TimeService.UtcNow);
								if (message != null)
								{
									return port;
								}
							}
							catch (TimeoutException)
							{
								// Try again due to timeout
								Thread.Sleep(1000);
							}
						}

						port.Close();
						port.Dispose();
					}
				}
				catch (Exception ex)
				{
					WriteLine(ex.Message);
				}
			}

			return null;
		}

		private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			var port = (SerialPort) sender;

			try
			{
				var line = port.ReadLine();
				var data = Encoding.UTF8.GetBytes(line);

				// Broadcast the data as a UDP broadcast
				_network.Send(data, data.Length, _networkEndPoint);

				var message = _parser.Parse(line, TimeService.UtcNow);

				if ((message == null) || !Options.VerboseLogging)
				{
					return;
				}

				WriteLine("\t\t >>> " + line);
				WriteLine(message.ToString());
			}
			catch (Exception ex)
			{
				WriteLine(ex.Message);
			}
		}

		#endregion
	}
}