#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Speedy.Automation.Desktop.Elements;
using Speedy.Automation.Internal;
using Speedy.Exceptions;

#endregion

namespace Speedy.Automation.Web.Browsers
{
	/// <summary>
	/// Represents a chromium based browser.
	/// </summary>
	public abstract class ChromiumBrowser : Browser
	{
		#region Constants

		/// <summary>
		/// The debugging argument for starting the browser.
		/// </summary>
		private const string DebugArgument = "--remote-debugging-port={0} --profile-directory=Speedy";

		#endregion

		#region Fields

		private static readonly HttpClient _client;

		private readonly int _debugPort;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private int _requestId;
		private ClientWebSocket _socket;
		private readonly ConcurrentDictionary<string, dynamic> _socketResponses;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Chrome class.
		/// </summary>
		/// <param name="port"> The debug port number for the browser. </param>
		/// <param name="application"> The window of the existing browser. </param>
		/// <param name="windowsToIgnore"> The windows to ignore. Optional. </param>
		protected ChromiumBrowser(int port, Application application, ICollection<IntPtr> windowsToIgnore = null)
			: base(application, windowsToIgnore)
		{
			_debugPort = port;
			_jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
			_requestId = 0;
			_socketResponses = new ConcurrentDictionary<string, dynamic>();
		}

		static ChromiumBrowser()
		{
			_client = new HttpClient();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Navigates the browser to the provided URI.
		/// </summary>
		/// <param name="uri"> The URI to navigate to. </param>
		protected override void BrowserNavigateTo(string uri)
		{
			var request = new Request
			{
				Id = _requestId++,
				Method = "Page.navigate",
				Params = new
				{
					Url = uri
				}
			};

			Application.Focus();
			Window.Focus();

			SendRequestAndReadResponse(request, x => x.id == request.Id);

			// todo: There must be a better way to determine when Chrome and Edge is done processing.
			Thread.Sleep(250);
		}

		/// <summary>
		/// Connect to the Chrome browser debugger port.
		/// </summary>
		/// <exception cref="Exception"> All debugging sessions are taken. </exception>
		protected void Connect()
		{
			var sessions = new List<RemoteSessionsResponse>();
			RemoteSessionsResponse session = null;

			Utility.Wait(() =>
			{
				try
				{
					sessions.AddRange(GetAvailableSessions());
					if (sessions.Count == 0)
					{
						throw new SpeedyException("All debugging sessions are taken.");
					}

					session = sessions.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.WebSocketDebuggerUrl));
					return session != null;
				}
				catch (WebException)
				{
					return false;
				}
			}, Application.Timeout.TotalMilliseconds, 250);

			if (session == null)
			{
				throw new SpeedyException("Could not find a valid debugger enabled page. Make sure you close the debugger tools.");
			}

			var sessionWsEndpoint = new Uri(session.WebSocketDebuggerUrl);
			_socket = new ClientWebSocket();

			if (!_socket.ConnectAsync(sessionWsEndpoint, CancellationToken.None).Wait(Application.Timeout))
			{
				throw new SpeedyException("Failed to connect to the server.");
			}

			Task.Run(() =>
			{
				while (_socket != null)
				{
					if (!ReadResponseAsync())
					{
						if (_socket == null)
						{
							break;
						}

						_socket?.Dispose();
						_socket = new ClientWebSocket();

						if (!_socket.ConnectAsync(sessionWsEndpoint, CancellationToken.None).Wait(Application.Timeout))
						{
							throw new SpeedyException("Failed to connect to the server.");
						}
					}

					Thread.Sleep(1);
				}
			});
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_socket?.Dispose();
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
			var request = new Request
			{
				Id = _requestId++,
				Method = "Runtime.evaluate",
				Params = new
				{
					DoNotPauseOnExceptions = true,
					Expression = script,
					ObjectGroup = "console",
					IncludeCommandLineAPI = true,
					ReturnByValue = expectResponse
				}
			};

			var data = SendRequestAndReadResponse(request, x => x.id == request.Id);
			if (data.Contains(SpeedyNotDefinedMessage))
			{
				return SpeedyNotDefinedMessage;
			}

			var response = data.AsJToken() as dynamic;
			if ((response == null) || (response.result == null) || (response.result.result == null))
			{
				return data;
			}

			var result = response?.result?.result;

			switch (result)
			{
				case JValue jValue:
				{
					return jValue.Value.ToString();
				}
				case JObject jObject:
				{
					return jObject.TryGetValue("value", out var value)
						? value.ToString()
						: JsonConvert.SerializeObject(result, Formatting.Indented);
				}
				default:
				{
					return result == null ? null : JsonConvert.SerializeObject(result, Formatting.Indented);
				}
			}
		}

		/// <summary>
		/// Reads the current URI directly from the browser.
		/// </summary>
		/// <returns> The current URI that was read from the browser. </returns>
		protected override string GetBrowserUri()
		{
			//LogManager.Write("First browser's URI.", LogLevel.Verbose);

			var request = new Request
			{
				Id = _requestId++,
				Method = "DOM.getDocument"
			};

			var response = SendRequestAndReadResponse(request, x => x.id == request.Id);
			var document = response.AsJToken() as dynamic;
			if ((document == null) || (document.result == null) || (document.result.root == null) || (document.result.root.documentURL == null))
			{
				throw new SpeedyException("Failed to get the URI.");
			}

			return document.result.root.documentURL;
		}

		/// <summary>
		/// Gets the debug arguments for the chromium browser.
		/// </summary>
		/// <param name="port"> The debug port number for the browser. </param>
		/// <returns> The command arguments for the browser. </returns>
		protected static string GetDebugArguments(int port)
		{
			return string.Format(DebugArgument, port);
		}

		/// <inheritdoc />
		protected override IScrollableElement GetScrollableElement()
		{
			return Application.FirstOrDefault<Document>(x => x.IsScrollable);
		}

		private List<RemoteSessionsResponse> GetAvailableSessions()
		{
			var location = $"http://127.0.0.1:{_debugPort}/json";
			var data = _client.GetStringAsync(location).Result;
			var sessions = JsonConvert.DeserializeObject<List<RemoteSessionsResponse>>(data);

			if (sessions == null)
			{
				throw new SpeedyException("Could not get available sessions.");
			}

			sessions.RemoveAll(x => x.Url.StartsWith("chrome-extension"));
			sessions.RemoveAll(x => x.Url.StartsWith("chrome-devtools"));
			sessions.RemoveAll(x => x.Url.StartsWith("chrome-untrusted"));
			sessions.RemoveAll(x => x.Title.Contains("https://ntp.msn.com/edge/ntp/service-worker.js"));

			if (sessions.Count > 1)
			{
				Debug.WriteLine("\r\nToo many sessions?");

				sessions.ForEach(x =>
				{
					Debug.WriteLine(x.Title);
					Debug.WriteLine(x.Url);
					Debug.WriteLine(x.DevtoolsFrontendUrl);
					Debug.WriteLine(x.WebSocketDebuggerUrl);
				});
			}

			return sessions;
		}

		private bool ReadResponse(int id)
		{
			return Utility.Wait(() => _socketResponses.ContainsKey(id.ToString()), Application.Timeout.TotalMilliseconds, 1);
		}

		private bool ReadResponseAsync()
		{
			var buffer = new ArraySegment<byte>(new byte[131072]);
			var builder = new StringBuilder();

			try
			{
				WebSocketReceiveResult result;

				if ((_socket.State == WebSocketState.Aborted) || (_socket.State == WebSocketState.Closed))
				{
					return false;
				}

				do
				{
					result = _socket.ReceiveAsync(buffer, CancellationToken.None).Result;
					var data = new byte[result.Count];
					Array.Copy(buffer.Array, 0, data, 0, data.Length);
					builder.Append(Encoding.UTF8.GetString(data));
				} while (!result.EndOfMessage);

				var response = (dynamic) builder.ToString().AsJToken();
				//LogManager.Write("Debugger Response: " + response, LogLevel.Verbose);

				if (response.id != null)
				{
					_socketResponses.TryAdd(response.id.ToString(), response);
				}

				return true;
			}
			catch (ObjectDisposedException)
			{
				return false;
			}
			catch (AggregateException)
			{
				return false;
			}
			catch
			{
				return false;
			}
		}

		private bool SendRequest<T>(T request)
		{
			var json = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
			//LogManager.Write("Debugger Request: " + json, LogLevel.Verbose);
			var jsonBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
			return _socket.SendAsync(jsonBuffer, WebSocketMessageType.Text, true, CancellationToken.None).Wait(Application.Timeout);
		}

		private string SendRequestAndReadResponse(Request request, Func<dynamic, bool> action)
		{
			if (SendRequest(request) && ReadResponse(request.Id))
			{
				_socketResponses.TryRemove(request.Id.ToString(), out var response);
				return response == null ? string.Empty : response.ToString();
			}

			request.Id++;
			return SendRequestAndReadResponse(request, action);
		}

		#endregion

		#region Classes

		[Serializable]
		[DataContract]
		internal class RemoteSessionsResponse
		{
			#region Properties

			[DataMember]
			public string DevtoolsFrontendUrl { get; set; }

			[DataMember]
			public string FaviconUrl { get; set; }

			[DataMember]
			public string Id { get; set; }

			[DataMember]
			public string ThumbnailUrl { get; set; }

			[DataMember]
			public string Title { get; set; }

			[DataMember]
			public string Url { get; set; }

			[DataMember]
			public string WebSocketDebuggerUrl { get; set; }

			#endregion
		}

		private class Request
		{
			#region Properties

			public int Id { get; set; }
			public string Method { get; set; }
			public dynamic Params { get; set; }

			#endregion
		}

		#endregion
	}
}