#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Automation.Desktop.Elements;
using Speedy.Automation.Web.Browsers;
using Speedy.Exceptions;

#endregion

namespace Speedy.Automation.Web
{
	/// <summary>
	/// This is the base class for browsers.
	/// </summary>
	public abstract class Browser : ElementHost, IScrollableElement
	{
		#region Constants

		/// <summary>
		/// Gets the missing Speedy error.
		/// </summary>
		public const string SpeedyNotDefinedMessage = "Speedy is not defined";

		#endregion

		#region Fields

		private string _lastUri;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the Browser class.
		/// </summary>
		protected Browser(Application application, ICollection<IntPtr> windowsToIgnore = null)
			: base(application, application)
		{
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			AutoRefresh = true;
			JavascriptLibraries = new JavaScriptLibrary[0];

			var watch = Stopwatch.StartNew();

			do
			{
				Window?.Dispose();
				Window = application.GetWindows(windowsToIgnore).FirstOrDefault();
			} while (((Window == null) || !Window.Visible) && (watch.Elapsed <= Timeout));
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current active element.
		/// </summary>
		public Element ActiveElement
		{
			get
			{
				var id = ExecuteScript("document.activeElement.id");
				if (string.IsNullOrWhiteSpace(id) || !Contains(id))
				{
					return null;
				}

				return FirstOrDefault(id);
			}
		}

		/// <summary>
		/// Gets or sets a flag to auto close the browser when disposed of. Defaults to false.
		/// </summary>
		public bool AutoClose
		{
			get => Application.AutoClose;
			set => Application.AutoClose = value;
		}

		/// <summary>
		/// Gets or sets a flag that allows elements to refresh when reading properties. Defaults to true.
		/// </summary>
		public bool AutoRefresh { get; set; }

		/// <summary>
		/// Gets the type of the browser.
		/// </summary>
		public abstract BrowserType BrowserType { get; }

		/// <inheritdoc />
		public override Element FocusedElement => ActiveElement;

		/// <inheritdoc />
		public double HorizontalScrollPercent => (ScrollableElement ?? (ScrollableElement = GetScrollableElement()))?.HorizontalScrollPercent ?? 0;

		/// <summary>
		/// Gets the ID of the browser.
		/// </summary>
		public override string Id => (Application?.Handle.ToInt32() ?? 0).ToString();

		/// <summary>
		/// Gets the value indicating if the browser is closed.
		/// </summary>
		public bool IsClosed => (Application == null) || !Application.IsRunning;

		/// <inheritdoc />
		public bool IsScrollable => (ScrollableElement ?? (ScrollableElement = GetScrollableElement()))?.IsScrollable ?? false;

		/// <summary>
		/// Gets a list of JavaScript libraries that were detected on the page.
		/// </summary>
		public IEnumerable<JavaScriptLibrary> JavascriptLibraries { get; set; }

		/// <summary>
		/// Gets the location of the browser.
		/// </summary>
		public Point Location => Application.Location;

		/// <summary>
		/// Gets the raw HTML of the page.
		/// </summary>
		public virtual string RawHtml => GetHtml();

		/// <summary>
		/// Gets the size of the browser.
		/// </summary>
		public Size Size => Application.Size;

		/// <summary>
		/// Gets or sets the time out for delay request. Defaults to 60 seconds.
		/// </summary>
		public TimeSpan Timeout
		{
			get => Application.Timeout;
			set => Application.Timeout = value;
		}

		/// <summary>
		/// Gets the URI of the current page.
		/// </summary>
		public string Uri => GetBrowserUri();

		/// <inheritdoc />
		public double VerticalScrollPercent => (ScrollableElement ?? (ScrollableElement = GetScrollableElement()))?.VerticalScrollPercent ?? 0;

		/// <summary>
		/// The main windows for the browser.
		/// </summary>
		public Window Window { get; }

		/// <summary>
		/// Gets the scrollable element to
		/// </summary>
		protected IScrollableElement ScrollableElement { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Attach browsers for each type provided.
		/// </summary>
		/// <param name="type"> The type of the browser to attach to. </param>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		public static IEnumerable<Browser> AttachBrowsers(BrowserType type = BrowserType.All, bool bringToFront = true)
		{
			var response = new List<Browser>();

			if ((type & BrowserType.Chrome) == BrowserType.Chrome)
			{
				response.Add(Chrome.Attach(bringToFront));
			}

			if ((type & BrowserType.Edge) == BrowserType.Edge)
			{
				response.Add(Edge.Attach(bringToFront));
			}

			if ((type & BrowserType.Firefox) == BrowserType.Firefox)
			{
				response.Add(Firefox.Attach(bringToFront));
			}

			return response;
		}

		/// <summary>
		/// Attach or create browsers for each type provided.
		/// </summary>
		/// <param name="type"> The type of the browser to attach to or create. </param>
		public static IEnumerable<Browser> AttachOrCreate(BrowserType type = BrowserType.All)
		{
			var response = new List<Browser>();

			if ((type & BrowserType.Chrome) == BrowserType.Chrome)
			{
				var chrome = Chrome.AttachOrCreate();
				response.Add(chrome);
			}

			if ((type & BrowserType.Edge) == BrowserType.Edge)
			{
				var edge = Edge.AttachOrCreate();
				response.Add(edge);
			}

			if ((type & BrowserType.Firefox) == BrowserType.Firefox)
			{
				var firefox = Firefox.AttachOrCreate();
				response.Add(firefox);
			}

			return response;
		}

		/// <summary>
		/// Attach process as a browser.
		/// </summary>
		/// <param name="process"> The process of the browser to attach to. </param>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		/// <returns> The browser if successfully attached or otherwise null. </returns>
		public static Browser AttachToBrowser(Process process, bool bringToFront = true)
		{
			return Chrome.Attach(process, bringToFront) ?? Edge.Attach(process, bringToFront) ?? Firefox.Attach(process, bringToFront);
		}

		/// <summary>
		/// Brings the application to the front and makes it the top window.
		/// </summary>
		public virtual Browser BringToFront()
		{
			Application.BringToFront();
			return this;
		}

		/// <summary>
		/// Closes the browser.
		/// </summary>
		public void Close()
		{
			Application?.Close();
		}

		/// <summary>
		/// Closes all browsers of the provided type.
		/// </summary>
		/// <param name="type"> The type of the browser to close. </param>
		public static void CloseBrowsers(BrowserType type = BrowserType.All)
		{
			if ((type & BrowserType.Chrome) == BrowserType.Chrome)
			{
				Application.CloseAll(Chrome.BrowserName);
			}

			if ((type & BrowserType.Edge) == BrowserType.Edge)
			{
				Application.CloseAll(Edge.BrowserName);
			}

			if ((type & BrowserType.Firefox) == BrowserType.Firefox)
			{
				Application.CloseAll(Firefox.BrowserName);
			}
		}

		/// <summary>
		/// Create browsers for each type provided.
		/// </summary>
		/// <param name="type"> The type of the browser to create. </param>
		/// <param name="bringToFront"> The option to bring the application to the front. This argument is optional and defaults to true. </param>
		public static IEnumerable<Browser> CreateBrowsers(BrowserType type = BrowserType.All, bool bringToFront = true)
		{
			var response = new List<Browser>();

			if ((type & BrowserType.Chrome) == BrowserType.Chrome)
			{
				response.Add(Chrome.Create(bringToFront));
			}

			if ((type & BrowserType.Edge) == BrowserType.Edge)
			{
				response.Add(Edge.Create());
			}

			if ((type & BrowserType.Firefox) == BrowserType.Firefox)
			{
				response.Add(Firefox.Create(bringToFront));
			}

			return response;
		}

		/// <summary>
		/// Execute JavaScript code in the current document.
		/// </summary>
		/// <param name="script"> The script to run. </param>
		/// <param name="expectResponse"> The script will return response. </param>
		/// <returns> The response from the script. </returns>
		public string ExecuteScript(string script, bool expectResponse = true)
		{
			var response = ExecuteJavaScript(script, expectResponse);

			// Check the response to see if the Speedy script has been injected.
			if (response?.Contains(SpeedyNotDefinedMessage) == true)
			{
				InjectTestScript();
				return ExecuteJavaScript(script, expectResponse);
			}

			return response;
		}

		/// <summary>
		/// Sets the browser as the focused window.
		/// </summary>
		public Browser Focus()
		{
			Window.Focus();
			Application.Focus();
			return this;
		}

		/// <summary>
		/// Process an action against a new instance of each browser type provided at the same time (parallel).
		/// </summary>
		/// <param name="action"> The action to perform against each browser. </param>
		/// <param name="type"> The type of the browser to process against. </param>
		/// <param name="timeout"> The timeout to wait for browsers to complete </param>
		public static void ForAllBrowsers(Action<Browser> action, BrowserType type, TimeSpan timeout)
		{
			var types = type.GetTypeArray();
			var tasks = types.Select(x => Task.Run(() =>
				{
					using var browser = AttachOrCreate(x).First();
					browser.Application.Timeout = timeout;
					action(browser);
				})
			).ToArray();

			// Add 20% time to ensure the thread doesn't timeout first.
			var milliseconds = (int) (timeout.TotalMilliseconds * 1.2);
			if (milliseconds < 10000)
			{
				milliseconds = 10000;
			}

			milliseconds = 1200000;

			var wait = Task.WaitAll(tasks, milliseconds);
			if (wait)
			{
				return;
			}

			// If we did timeout then capture the exceptions and throw a timeout error
			var errors = tasks.Where(x => x.Exception != null).Select(x => x.Exception.Message);
			var error = string.Join(Environment.NewLine, errors);

			if (string.IsNullOrWhiteSpace(error))
			{
				throw new SpeedyException($"{nameof(ForAllBrowsers)} has timed out.");
			}

			throw new SpeedyException($"{nameof(ForAllBrowsers)} has timed out.{error}");
		}

		/// <summary>
		/// Process an action against a new instance of each browser type provided (serial).
		/// </summary>
		/// <param name="action"> The action to perform against each browser. </param>
		/// <param name="type"> The type of the browser to process against. </param>
		public static void ForEachBrowser(Action<Browser> action, BrowserType type = BrowserType.All)
		{
			var browsers = AttachOrCreate(type);
			foreach (var browser in browsers)
			{
				using (browser)
				{
					action(browser);
				}
			}
		}

		/// <summary>
		/// Gets the HTML displayed in the browser.
		/// </summary>
		public string GetHtml()
		{
			return ExecuteScript("document.documentElement.outerHTML")
				.Replace("<input id=\"speedyResult\" type=\"hidden\" value=\"\">", "");
		}

		/// <summary>
		/// Inserts the test script into the current page.
		/// </summary>
		public static string GetTestScript()
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (var stream = assembly.GetManifestResourceStream("Speedy.Automation.Speedy.Automation.js"))
			{
				if (stream != null)
				{
					using (var reader = new StreamReader(stream))
					{
						var data = reader.ReadToEnd();
						var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						for (var i = 0; i < lines.Length; i++)
						{
							lines[i] = lines[i].Trim();
						}

						data = string.Join("", lines.Where(x => !x.StartsWith("//"))).Replace("\t", "");
						return data;
					}
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Move the window and resize it.
		/// </summary>
		/// <param name="x"> The x coordinate to move to. </param>
		/// <param name="y"> The y coordinate to move to. </param>
		public virtual Browser MoveWindow(int x, int y)
		{
			Application.Focus();
			Application.MoveWindow(x, y);
			Window.Focus();
			Window.Move(x, y);
			return this;
		}

		/// <summary>
		/// Move the window and resize it.
		/// </summary>
		/// <param name="x"> The x coordinate to move to. </param>
		/// <param name="y"> The y coordinate to move to. </param>
		/// <param name="width"> The width of the window. </param>
		/// <param name="height"> The height of the window. </param>
		public virtual Browser MoveWindow(int x, int y, int width, int height)
		{
			Application.Focus();
			Application.MoveWindow(x, y, width, height);
			Window.Focus();
			Window.Move(x, y, width, height);
			return this;
		}

		/// <summary>
		/// Move the window and resize it.
		/// </summary>
		/// <param name="location"> The location to move to. </param>
		/// <param name="size"> The size of the window. </param>
		public virtual Browser MoveWindow(Point location, Size size)
		{
			Application.Focus();
			Application.MoveWindow(location, size);
			Window.Focus();
			Window.Move(location, size);
			return this;
		}

		/// <summary>
		/// Navigates the browser to the provided URI.
		/// </summary>
		/// <param name="uri"> The URI to navigate to. </param>
		/// <param name="expectedUri"> The expected URI to navigate to. </param>
		public Browser NavigateTo(string uri, string expectedUri = null)
		{
			if (uri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}

			_lastUri = Uri;

			try
			{
				//LogManager.Write("Navigating to " + expectedUri + ".", LogLevel.Verbose);
				BrowserNavigateTo(uri);

				if (_lastUri.Equals(uri, StringComparison.OrdinalIgnoreCase))
				{
					Refresh();
					return this;
				}

				WaitForNavigation(expectedUri ?? uri);
				return this;
			}
			finally
			{
				_lastUri = uri;
			}
		}

		/// <inheritdoc />
		public override ElementHost Refresh<T>(Func<T, bool> condition)
		{
			WaitForComplete();
			InjectTestScript();
			DetectJavascriptLibraries();
			RefreshElements();
			return this;
		}

		/// <summary>
		/// Removes the element from the page. * Experimental
		/// </summary>
		/// <param name="element"> The element to remove. </param>
		public bool RemoveElement(WebElement element)
		{
			ExecuteJavaScript($"Speedy.removeElement(\'{element.Id}\',{element.GetFrameIdInsert()});", false);
			return Children.Remove(element);
		}

		/// <summary>
		/// Removes an attribute from an element.
		/// </summary>
		/// <param name="element"> The element to remove the attribute from. </param>
		/// <param name="name"> The name of the attribute to remove. </param>
		public void RemoveElementAttribute(WebElement element, string name)
		{
			ExecuteJavaScript($"Speedy.removeElementAttribute(\'{element.Id}\',{element.GetFrameIdInsert()},\'{name}\');", false);
		}

		/// <summary>
		/// Resize the browser to the provided size.
		/// </summary>
		/// <param name="width"> The width to set. </param>
		/// <param name="height"> The height to set. </param>
		public ElementHost Resize(int width, int height)
		{
			Application.Resize(width, height);
			return this;
		}

		/// <summary>
		/// Scroll the browser window.
		/// </summary>
		/// <param name="horizontalPercent"> The percentage to scroll horizontally. </param>
		/// <param name="verticalPercent"> The percentage to scroll vertically. </param>
		public void Scroll(double horizontalPercent, double verticalPercent)
		{
			try
			{
				var e = ScrollableElement ?? (ScrollableElement = GetScrollableElement());
				e?.Scroll(horizontalPercent, verticalPercent);
			}
			catch (Exception)
			{
				Application.Children.Clear();

				var e = ScrollableElement = GetScrollableElement();
				e?.Scroll(horizontalPercent, verticalPercent);
			}
		}

		/// <summary>
		/// Sets the HTML to display in the browser.
		/// </summary>
		/// <param name="html"> The HTML to apply to the browser. </param>
		public void SetHtml(string html)
		{
			var innerHtml = CleanupScriptForJavascriptString(html);
			ExecuteScript($"document.open(); document.write('{innerHtml}'); document.close();", false);
		}

		/// <inheritdoc />
		public override ElementHost WaitForComplete(int minimumDelay = 0)
		{
			Wait(x => ExecuteJavaScript("document.readyState === 'complete'").Equals("true", StringComparison.OrdinalIgnoreCase));
			Application?.WaitForComplete(minimumDelay);
			return this;
		}

		/// <summary>
		/// Wait for the browser page to redirect to a provided URI.
		/// </summary>
		/// <param name="uri"> The expected URI to land on. Defaults to empty string if not provided. </param>
		/// <param name="timeout"> The timeout before giving up on the redirect. Defaults to Timeout if not provided. </param>
		public Browser WaitForNavigation(string uri = null, TimeSpan? timeout = null)
		{
			if (timeout == null)
			{
				timeout = Application.Timeout;
			}

			if (uri == null)
			{
				//LogManager.Write("Waiting for navigation with timeout of " + timeout.Value + ".", LogLevel.Verbose);
				if (!Wait(x => Uri != _lastUri, (int) timeout.Value.TotalMilliseconds))
				{
					throw new SpeedyException($"Browser never completed navigated away from {Uri}.");
				}
			}
			else
			{
				//LogManager.Write("Waiting for navigation to " + uri + " with timeout of " + timeout.Value + ".", LogLevel.Verbose);
				var alternateUri = uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? "https://" + uri.Substring(7) : "http://" + uri.Substring(8);
				if (!Wait(x => Uri.StartsWith(uri, StringComparison.OrdinalIgnoreCase)
							|| Uri.StartsWith(alternateUri, StringComparison.OrdinalIgnoreCase),
						(int) timeout.Value.TotalMilliseconds))
				{
					throw new SpeedyException($"Browser never completed navigation to {uri}. Current URI is {Uri}.");
				}
			}

			Refresh();
			return this;
		}

		/// <summary>
		/// Browser implementation of navigate to
		/// </summary>
		/// <param name="uri"> The URI to navigate to. </param>
		protected abstract void BrowserNavigateTo(string uri);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			if (AutoClose)
			{
				Application?.Close();
			}

			Application?.Dispose();
		}

		/// <summary>
		/// Execute JavaScript code in the current document.
		/// </summary>
		/// <param name="script"> The code script to execute. </param>
		/// <param name="expectResponse"> The script will return response. </param>
		/// <returns> The response from the execution. </returns>
		protected abstract string ExecuteJavaScript(string script, bool expectResponse = true);

		/// <summary>
		/// Reads the current URI directly from the browser.
		/// </summary>
		/// <returns> The current URI that was read from the browser. </returns>
		protected abstract string GetBrowserUri();

		/// <summary>
		/// Gets the scrollable element to scroll browser content.
		/// </summary>
		/// <returns> </returns>
		protected abstract IScrollableElement GetScrollableElement();

		/// <summary>
		/// Injects the test script into the browser.
		/// </summary>
		/// <param name="count"> The count of how many times this method has been called. </param>
		protected void InjectTestScript(int count = 0)
		{
			if (count > 3)
			{
				throw new SpeedyException("Failed to inject the Speedy JavaScript.");
			}

			ExecuteJavaScript(GetTestScript());

			var test = ExecuteJavaScript("typeof Speedy");
			if (test.Equals("undefined"))
			{
				InjectTestScript(count + 1);
			}
		}

		/// <summary>
		/// Replaces a character with a string. Does not replace if the string replacement matches at starting character.
		/// </summary>
		/// <param name="input"> The string to parse. </param>
		/// <param name="replacements"> The collection of trigger characters and replacements. </param>
		/// <returns> The updated string. </returns>
		protected string Replace(string input, Dictionary<char, string> replacements)
		{
			foreach (var r in replacements)
			{
				for (var i = 0; i < input.Length; i++)
				{
					if (input[i] != r.Key)
					{
						continue;
					}

					if ((i + r.Value.Length) < input.Length)
					{
						if (input.Substring(i, r.Value.Length) != r.Value)
						{
							input = input.Remove(i, 1).Insert(i, r.Value);
						}

						i += r.Value.Length - 1;
					}
				}
			}

			return input;
		}

		/// <summary>
		/// Replaces a character with a string processing input in reverse. Does not replace if the string replacement matches at starting character.
		/// </summary>
		/// <param name="input"> The string to parse. </param>
		/// <param name="replacements"> The collection of trigger characters and replacements. </param>
		/// <returns> The updated string. </returns>
		protected string ReplaceInReverse(string input, Dictionary<char, string> replacements)
		{
			foreach (var r in replacements)
			{
				for (var i = input.Length - 1; i > 0; i--)
				{
					if (input[i] != r.Key)
					{
						continue;
					}

					if (((i - r.Value.Length) >= 0) && (input.Substring(i - r.Value.Length, r.Value.Length) != r.Value))
					{
						input = input.Remove(i, 1).Insert(i, r.Value);
					}
				}
			}

			return input;
		}

		internal string CleanupScriptForJavascriptString(string html)
		{
			return ReplaceInReverse(html, new Dictionary<char, string> { { '\'', "\\'" }, { '\"', "\\\"" }, { '\n', "\\n" }, { '\r', "\\r" }, { '\0', "\\0" } });
		}

		internal ICollection<WebElement> GetElements(WebElement parent = null)
		{
			var data = ExecuteScript(parent?.Id == null ? $"JSON.stringify(Speedy.getElements(undefined,{parent?.GetFrameIdInsert() ?? "undefined"}))" : $"JSON.stringify(Speedy.getElements('{parent.Id}',{parent.GetFrameIdInsert()}))");
			if (JavascriptLibraries.Contains(JavaScriptLibrary.Angular) && data.Contains("ng-view ng-cloak"))
			{
				throw new SpeedyException("JavaScript not completed?");
			}

			var elements = JsonConvert.DeserializeObject<JArray>(data);
			return elements?.Select(x => WebElement.Create(x, this, parent)).ToList() ?? new List<WebElement>();
		}

		/// <summary>
		/// Runs script to detect specific libraries.
		/// </summary>
		private void DetectJavascriptLibraries()
		{
			//LogManager.Write("Detecting JavaScript libraries.", LogLevel.Verbose);

			var uri = GetBrowserUri();
			if ((uri.Length <= 0) || uri.Equals("about:tabs"))
			{
				return;
			}

			var libraries = new List<JavaScriptLibrary>();
			var hasLibrary = ExecuteScript("typeof jQuery !== 'undefined'");
			if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				libraries.Add(JavaScriptLibrary.JQuery);
			}

			hasLibrary = ExecuteScript("typeof Vue !== 'undefined'");
			if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				libraries.Add(JavaScriptLibrary.Vue);
			}

			hasLibrary = ExecuteScript("typeof angular !== 'undefined'");
			if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				libraries.Add(JavaScriptLibrary.Angular);
			}

			hasLibrary = ExecuteScript("typeof moment !== 'undefined'");
			if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				libraries.Add(JavaScriptLibrary.Moment);
			}

			// neither of the bootstrap tests are guaranteed since it's possible to customize 
			// bootstrap to not include some plugins. Bootstrap doesn't provide a namespace to
			// test against, so there is a small change of a false positive
			hasLibrary = ExecuteScript("typeof $().emulateTransitionEnd == 'function'");
			if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				libraries.Add(JavaScriptLibrary.Bootstrap3);
			}

			// both bs 2 and 3 have popover but only 3+ uses emulateTransitionEnd
			if (!libraries.Contains(JavaScriptLibrary.Bootstrap3))
			{
				hasLibrary = ExecuteScript("typeof($.fn.popover) !== 'undefined'");
				if (hasLibrary.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					libraries.Add(JavaScriptLibrary.Bootstrap2);
				}
			}

			JavascriptLibraries = libraries;
		}

		/// <summary>
		/// Refreshes the element collection for the current page.
		/// </summary>
		private void RefreshElements()
		{
			//LogManager.Write("Refresh the elements.", LogLevel.Verbose);
			Children.Clear();

			var elements = GetElements();
			var elementLookup = elements.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.First());
			var parents = new Dictionary<string, Element>();

			// Locate parents
			foreach (var element in elements)
			{
				if (string.IsNullOrWhiteSpace(element.ParentId))
				{
					parents.Add(element.Id, element);
				}
				else
				{
					var parent = elementLookup.ContainsKey(element.ParentId) ? elementLookup[element.ParentId] : null;
					parent?.Children.Add(element);
					element.Parent = parent;
				}
			}

			Children.AddRange(parents.Values);
		}

		#endregion
	}
}