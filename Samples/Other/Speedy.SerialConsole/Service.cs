#region References

using System;
using System.Diagnostics.Tracing;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Speedy.Extensions;
using Speedy.ServiceHosting;

#endregion

namespace Speedy.SerialConsole
{
	public class Service : WindowsService<ServiceOptions>
	{
		#region Fields

		private TcpClient _client;
		private NetworkStream _clientStream;
		private TcpListener _listener;

		#endregion

		#region Constructors

		public Service(ServiceOptions options) : base(options)
		{
		}

		#endregion

		#region Methods

		protected override void Process()
		{
			SerialPort port = null;

			_listener = new TcpListener(IPAddress.Parse(Options.NetworkAddress), Options.NetworkPort);
			_listener.Start();

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

			if (port == null)
			{
				WriteLine("Failed to find the serial port.");
			}
			else
			{
				port.ReadTimeout = Options.Timeout;
				WriteLine($"Found serial device on port {port.PortName}:{port.BaudRate}, Read Timeout: {port.ReadTimeout}");
				port.DataReceived += PortOnDataReceived;
			}

			while (IsRunning)
			{
				Console.WriteLine($"Listening on {Options.NetworkAddress}:{Options.NetworkPort}");
				Console.WriteLine("Waiting for a connection... ");

				// Perform a blocking call to accept requests.
				WaitForClient(port);
			}
		}

		protected override void WriteLine(string message, EventLevel level = EventLevel.Informational)
		{
			Console.WriteLine(message);
			base.WriteLine(message, level);
		}

		private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			var port = (SerialPort) sender;

			try
			{
				var buffer = new byte[1024];
				var read = 0;

				while (port.BytesToRead > 0)
				{
					buffer[read++] = (byte) port.ReadByte();
					Thread.Sleep(1);
				}

				var message = buffer.ToHexString(0, read);
				WriteLine("\t >>> " + message);

				_clientStream?.Write(buffer, 0, read);
			}
			catch (Exception ex)
			{
				WriteLine(ex.Message);
			}
			finally
			{
				_clientStream?.Close();
				_clientStream?.Dispose();
				_clientStream = null;
			}
		}

		private void WaitForClient(SerialPort port)
		{
			try
			{
				_client = _listener.AcceptTcpClient();

				Console.WriteLine("Connected!");

				// Get a stream object for reading and writing
				_clientStream = _client.GetStream();

				var bytes = new byte[1024];
				int bytesRead;

				// Loop to receive all the data sent by the client.
				if ((bytesRead = _clientStream.Read(bytes, 0, bytes.Length)) != 0)
				{
					// Translate data bytes to a hex array string.
					var message = bytes.ToHexString(0, bytesRead);
					Console.WriteLine("Received: {0}", message);

					//_clientStream?.Write(bytes, 0, bytesRead);
					port.Write(bytes, 0, bytesRead);

					UtilityExtensions.WaitUntil(() => _clientStream == null, 5000, 25);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				_clientStream?.Close();
				_clientStream?.Dispose();
				_clientStream = null;
			}
		}

		#endregion
	}
}