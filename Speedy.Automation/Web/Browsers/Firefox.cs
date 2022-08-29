#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speedy.Automation.Internal;
using Speedy.Exceptions;

#endregion

namespace Speedy.Automation.Web.Browsers
{
	/// <summary>
	/// Represents a Firefox browser.
	/// </summary>
	public class Firefox : Browser
	{
		#region Constants

		/// <summary>
		/// The name of the browser.
		/// </summary>
		public const string BrowserName = "firefox";

		/// <summary>
		/// The debugging argument for starting the browser.
		/// </summary>
		public const string DebugArgument = "-start-debugger-server 6000";

		#endregion

		#region Fields

		private string _consoleActor;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly FirefoxBuffer _messageBuffer;
		private readonly List<dynamic> _responses;
		private Socket _socket;
		private readonly byte[] _socketBuffer;
		private string _tabActor;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Firefox class.
		/// </summary>
		/// <param name="application"> The window of the existing browser. </param>
		/// <param name="windowsToIgnore"> The windows to ignore. Optional. </param>
		private Firefox(Application application, ICollection<IntPtr> windowsToIgnore = null)
			: base(application, windowsToIgnore)
		{
			_consoleActor = string.Empty;
			_jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			_messageBuffer = new FirefoxBuffer();
			_responses = new List<dynamic>();
			_socketBuffer = new byte[FirefoxBuffer.InitialSize];
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the type of the browser.
		/// </summary>
		public override BrowserType BrowserType => BrowserType.Firefox;

		#endregion

		#region Methods

		/// <summary>
		/// Attempts to attach to an existing browser.
		/// </summary>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance or null if not found. </returns>
		public static Browser Attach(bool bringToFront = true)
		{
			var application = Application.Attach(BrowserName, null, false, bringToFront);
			if (application == null)
			{
				return null;
			}

			var browser = new Firefox(application);
			browser.Connect();
			browser.Refresh();
			return browser;
		}

		/// <summary>
		/// Attempts to attach to an existing browser.
		/// </summary>
		/// <param name="process"> The process to attach to. </param>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance or null if not found. </returns>
		public static Browser Attach(Process process, bool bringToFront = true)
		{
			if (process.ProcessName != BrowserName)
			{
				return null;
			}

			if (!Application.Exists(BrowserName, DebugArgument))
			{
				throw new ArgumentException("The process was not started with the debug arguments.", nameof(process));
			}

			var application = Application.Attach(process, false, bringToFront);
			var browser = new Firefox(application);
			browser.Connect();
			browser.Refresh();
			return browser;
		}

		/// <summary>
		/// Attempts to attach to an existing browser. If one is not found then create and return a new one.
		/// </summary>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance. </returns>
		public static Browser AttachOrCreate(bool bringToFront = true)
		{
			return Attach(bringToFront) ?? Create(bringToFront);
		}

		/// <summary>
		/// Attempts to create a new browser.
		/// </summary>
		/// <remarks>
		/// The Firefox browser must have the "listen 6000" command run in the console to enable remote debugging. A newly created
		/// browser will not be able to connect until someone manually starts the remote debugger.
		/// </remarks>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser instance. </returns>
		public static Browser Create(bool bringToFront = true)
		{
			Firefox browser;

			// See if Firefox is already running
			var application = Application.Attach(BrowserName, DebugArgument, false, bringToFront);
			if (application != null)
			{
				// Start process but connect to current process and new window.
				var existing = application.GetWindows().Select(x =>
				{
					x.Dispose();
					return x.Handle;
				}).ToList();

				try
				{
					Application.Create(BrowserName, "-new-window", false, bringToFront).Dispose();
				}
				catch
				{
					// Process may close during creation causing error.
				}

				browser = new Firefox(application, existing);
			}
			else
			{
				application = Application.Create(BrowserName, DebugArgument, false, bringToFront);
				browser = new Firefox(application);
			}

			browser.Connect();
			browser.NavigateTo("about:blank");
			return browser;
		}

		/// <summary>
		/// Navigates the browser to the provided URI.
		/// </summary>
		/// <param name="uri"> The URI to navigate to. </param>
		protected override void BrowserNavigateTo(string uri)
		{
			ExecuteJavaScript(Uri == uri ? "window.location.reload(true)" : $"window.location.href = \"{uri}\"");
			SendRequest("Wake up, Neo...");

			// todo: There must be a better way to determine when Chrome and Firefox is done processing.
			Thread.Sleep(250);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (_socket != null))
			{
				_socket.Dispose();
				_socket = null;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Execute JavaScript code in the current document.
		/// </summary>
		/// <param name="script"> The code script to execute. </param>
		/// <param name="expectResponse"> The script will return response. </param>
		/// <returns> The response from the execution. </returns>
		protected override string ExecuteJavaScript(string script, bool expectResponse = true)
		{
			var request = new { To = _consoleActor, Type = "evaluateJSAsync", Text = script };
			var response = SendRequestAndReadResponse(request, x => (x.from == _consoleActor) && (x.input == script));

			if (!string.IsNullOrEmpty(((object) response.exception)?.ToString()))
			{
				return SpeedyNotDefinedMessage;
			}

			var result = ((object) response.result).ToString();
			return result.Contains("\"type\": \"null\"") ? null : result.Contains("\"type\": \"longString\"") ? ReadLongResponse(result) : result;
		}

		/// <summary>
		/// Reads the current URI directly from the browser.
		/// </summary>
		/// <returns> The current URI that was read from the browser. </returns>
		protected override string GetBrowserUri()
		{
			//LogManager.Write("First browser's URI.", LogLevel.Verbose);
			return ExecuteJavaScript("window.location.href");
		}

		/// <inheritdoc />
		protected override IScrollableElement GetScrollableElement()
		{
			return null;
		}

		/// <summary>
		/// Connect to the Firefox browser debugger port.
		/// </summary>
		/// <exception cref="Exception"> All debugging sessions are taken. </exception>
		private void Connect()
		{
			Utility.Wait(() =>
			{
				try
				{
					_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					_socket.Connect("127.0.0.1", 6000);
					return true;
				}
				catch
				{
					return false;
				}
			}, Application.Timeout.TotalMilliseconds, 10);

			Task.Run(() =>
			{
				//LogManager.Write("Firefox: Read thread is starting...", LogLevel.Verbose);

				while (ReadResponseAsync())
				{
					Thread.Sleep(1);
				}

				//LogManager.Write("Firefox: Read thread is closing...", LogLevel.Verbose);
			});

			// Wait for the connect response.
			WaitForResponse(x => (x.from == "root") && (x.applicationType == "browser"));

			// Initialize the actor references.
			InitializeActors();
		}

		private void InitializeActors()
		{
			var listTabRequest = new { To = "root", Type = "listTabs" };
			var listTabResponse = SendRequestAndReadResponse(listTabRequest, x => (x.from == "root") && (x.tabs != null));

			while (listTabResponse.tabs.Count <= 0)
			{
				Thread.Sleep(10);
				listTabResponse = SendRequestAndReadResponse(listTabRequest, x => (x.from == "root") && (x.tabs != null));
			}

			var tabs = (IEnumerable<dynamic>) listTabResponse.tabs;
			var selected = tabs.FirstOrDefault(x => x.selected);
			_consoleActor = selected.consoleActor;
			_tabActor = selected.actor;

			var attachTabRequest = new { To = _tabActor, Type = "attach" };
			SendRequestAndReadResponse(attachTabRequest, x => (x.from == _tabActor) && (x.type == "tabAttached"));
		}

		private string ReadLongResponse(string response)
		{
			var result = (dynamic) JsonConvert.DeserializeObject(response);
			if (result.type != "longString")
			{
				throw new SpeedyException("This response was not a long string response.");
			}

			var length = (int) result.length;
			var builder = new StringBuilder(length);
			var offset = builder.Length;
			var chuckLength = 131070;
			var actor = (string) result.actor;

			while (offset < length)
			{
				SendRequest(new { To = actor, Type = "substring", Start = offset, End = offset + chuckLength });
				var subresult = WaitForResponse(x => (x.from == actor) && (x.substring != null));
				builder.Append((string) subresult.substring);
				offset += chuckLength;
			}

			return builder.ToString();
		}

		private bool ReadResponseAsync()
		{
			try
			{
				if (_socket == null)
				{
					return false;
				}

				var result = _socket.Receive(_socketBuffer);
				_messageBuffer.Add(_socketBuffer, result);

				var messages = _messageBuffer.GetMessages();
				foreach (var message in messages)
				{
					try
					{
						var token = message.AsJToken() as dynamic;
						//LogManager.Write("Debugger Response: " + message, LogLevel.Verbose);

						if (message.Contains("\"type\":\"tabNavigated\""))
						{
							continue;
						}

						lock (_responses)
						{
							_responses.Add(token);
						}
					}
					catch
					{
						//LogManager.Write("Invalid message! " + messages, LogLevel.Fatal);
					}
				}

				return true;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
			catch (SocketException)
			{
				return false;
			}
			catch (Exception)
			{
				//LogManager.Write(ex.Message, LogLevel.Fatal);
				return false;
			}
		}

		private void SendRequest<T>(T request)
		{
			var json = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
			var data = json.Length + ":" + json;
			//LogManager.Write("Debugger Request: " + data, LogLevel.Verbose);
			var jsonBuffer = Encoding.UTF8.GetBytes(data);
			_socket.Send(jsonBuffer);
		}

		private dynamic SendRequestAndReadResponse(dynamic request, Func<dynamic, bool> action)
		{
			lock (_responses)
			{
				_responses.Clear();
			}

			SendRequest(request);
			return WaitForResponse(action);
		}

		private dynamic WaitForResponse(Func<dynamic, bool> action)
		{
			dynamic result = null;

			Utility.Wait(() =>
			{
				lock (_responses)
				{
					for (var i = 0; i < _responses.Count; i++)
					{
						if (!action(_responses[i]))
						{
							continue;
						}

						result = _responses[i];
						return true;
					}

					return false;
				}
			}, Application.Timeout.TotalMilliseconds, 1);

			if (result == null)
			{
				return null;
			}

			lock (_responses)
			{
				_responses.Remove(result);
				return result;
			}
		}

		#endregion

		#region Classes

		private class FirefoxBuffer
		{
			#region Constants

			public const int InitialSize = 131070;

			#endregion

			#region Fields

			private readonly List<byte> _buffer;

			#endregion

			#region Constructors

			public FirefoxBuffer()
			{
				_buffer = new List<byte>(InitialSize);
			}

			#endregion

			#region Methods

			public void Add(byte[] buffer, int length)
			{
				var data = new byte[length];
				Array.Copy(buffer, 0, data, 0, data.Length);
				_buffer.AddRange(data);
			}

			public IEnumerable<string> GetMessages()
			{
				var response = new List<string>();
				var message = ReadNextMessage();

				while (message != null)
				{
					response.Add(message);
					message = ReadNextMessage();
				}

				return response;
			}

			private byte[] Read(int index, int length)
			{
				if (index >= _buffer.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				if (length > _buffer.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(length));
				}

				if ((index + length) > _buffer.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(length));
				}

				var response = new byte[length];
				Array.Copy(_buffer.ToArray(), index, response, 0, response.Length);
				return response;
			}

			private string ReadNextMessage()
			{
				var index = _buffer.IndexOf(58);
				if (index == -1)
				{
					return null;
				}

				var lengthString = Encoding.UTF8.GetString(Read(0, index));
				var length = int.Parse(lengthString);
				if (_buffer.Count < (index + 1 + length))
				{
					return null;
				}

				var response = Encoding.UTF8.GetString(Read(index + 1, length));
				_buffer.RemoveRange(0, index + 1 + length);
				return response;
			}

			#endregion
		}

		#endregion
	}
}