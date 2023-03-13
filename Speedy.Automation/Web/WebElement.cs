#region References

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Automation.Desktop;
using Speedy.Automation.Web.Elements;
using Keyboard = Speedy.Automation.Web.Elements.Keyboard;
using Object = Speedy.Automation.Web.Elements.Object;

#endregion

namespace Speedy.Automation.Web;

/// <summary>
/// Represents an element for a browser.
/// </summary>
public class WebElement : Element
{
	#region Fields

	private readonly dynamic _element;
	private readonly string _highlightColor;
	private readonly string _originalColor;

	/// <summary>
	/// Properties that need to be renamed when requested.
	/// </summary>
	private static readonly Dictionary<string, string> _propertiesToRename;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes an instance of a browser element.
	/// </summary>
	/// <param name="element"> The browser element this is for. </param>
	/// <param name="browser"> The browser this element is associated with. </param>
	/// <param name="parent"> </param>
	protected WebElement(JToken element, Browser browser, ElementHost parent)
		: base(browser.Application, parent)
	{
		Browser = browser;
		_element = element;
		_originalColor = GetStyleAttributeValue("backgroundColor", false) ?? "";
		_highlightColor = "yellow";
	}

	static WebElement()
	{
		_propertiesToRename = new Dictionary<string, string> { { "class", "className" } };
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets or sets the access key attribute.
	/// </summary>
	/// <remarks>
	/// Specifies a shortcut key to activate/focus an element.
	/// </remarks>
	public string AccessKey
	{
		get => this["accesskey"];
		set => this["accesskey"] = value;
	}

	/// <inheritdoc />
	public override string AutomationId => Id;

	/// <summary>
	/// Gets the browser this element is currently associated with.
	/// </summary>
	public Browser Browser { get; }

	/// <summary>
	/// Gets or sets the class attribute.
	/// </summary>
	/// <remarks>
	/// Specifies one or more class names for an element (refers to a class in a style sheet).
	/// </remarks>
	public string Class
	{
		get => this["class"];
		set => this["class"] = value;
	}

	/// <summary>
	/// Gets the content editable (contenteditable) attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies whether the content of an element is editable or not.
	/// </remarks>
	public string ContentEditable
	{
		get => this["contenteditable"];
		set => this["contenteditable"] = value;
	}

	/// <summary>
	/// Gets or sets the context menu attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies a context menu for an element. The context menu appears when a user Right-clicks on the element.
	/// </remarks>
	public string ContextMenu
	{
		get => this["contextmenu"];
		set => this["contextmenu"] = value;
	}

	/// <summary>
	/// Gets or sets the draggable attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies whether an element is draggable or not.
	/// </remarks>
	public string Draggable
	{
		get => this["draggable"];
		set => this["draggable"] = value;
	}

	/// <summary>
	/// Gets or sets the drop zone (dropzone) attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies whether the dragged data is copied, moved, or linked, when dropped.
	/// </remarks>
	public string DropZone
	{
		get => this["dropzone"];
		set => this["dropzone"] = value;
	}

	/// <inheritdoc />
	public override bool Enabled => this["disabled"] != "true";

	/// <inheritdoc />
	public override bool Focused => Browser.FocusedElement == this;

	/// <inheritdoc />
	public override Element FocusedElement => Browser.FocusedElement;

	/// <summary>
	/// Gets the optional ID of the frame hosting the element. Will be null if the element is not hostied in a frame.
	/// </summary>
	public string FrameId => _element.frameId;

	/// <inheritdoc />
	public override int Height => (int) _element.height;

	/// <summary>
	/// Gets or sets the hidden attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies that an element is not yet, or is no longer, relevant.
	/// </remarks>
	public string Hidden
	{
		get => this["hidden"];
		set => this["hidden"] = value;
	}

	/// <inheritdoc />
	public override string Id => _element.id;

	/// <inheritdoc />
	public override string this[string name]
	{
		get => GetAttributeValue(name, Browser.AutoRefresh);
		set => SetAttributeValue(name, value);
	}

	/// <summary>
	/// Gets or sets the language (lang) attribute.
	/// </summary>
	/// <remarks>
	/// Specifies the language of the element's content.
	/// </remarks>
	public string Language
	{
		get => this["lang"];
		set => this["lang"] = value;
	}

	/// <inheritdoc />
	public override Point Location
	{
		get
		{
			var data = Browser.ExecuteScript($"Speedy.getElementLocation(\'{Id}\',{GetFrameIdInsert()});");
			var result = JsonConvert.DeserializeObject<dynamic>(data);
			return new Point((int) result.x, (int) result.y);
		}
	}

	/// <inheritdoc />
	public override string Name => _element.name;

	/// <summary>
	/// Gets the ID of the parent element.
	/// </summary>
	public string ParentId => _element.parentId;

	/// <summary>
	/// Gets or sets the spell check (spellcheck) attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies whether the element is to have its spelling and grammar checked or not.
	/// </remarks>
	public string SpellCheck
	{
		get => this["spellcheck"];
		set => this["spellcheck"] = value;
	}

	/// <summary>
	/// Gets or sets the style attribute.
	/// </summary>
	/// <remarks>
	/// Specifies an inline CSS style for an element.
	/// </remarks>
	public string Style
	{
		get => this["style"];
		set => this["style"] = value;
	}

	/// <summary>
	/// Gets or sets the tab index (tabindex) attribute.
	/// </summary>
	/// <remarks>
	/// Specifies the tabbing order of the element.
	/// </remarks>
	public string TabIndex
	{
		get => this["tabindex"];
		set => this["tabindex"] = value;
	}

	/// <summary>
	/// Gets the tag element name.
	/// </summary>
	public string TagName => _element.tagName;

	/// <summary>
	/// Gets or sets the text content.
	/// </summary>
	public virtual string Text
	{
		get => this["textContent"];
		set => this["textContent"] = value;
	}

	/// <summary>
	/// Gets the text direction (dir) attribute.
	/// </summary>
	/// <remarks>
	/// Specifies the text direction for the content in an element.
	/// </remarks>
	public string TextDirection
	{
		get => this["dir"];
		set => this["dir"] = value;
	}

	/// <summary>
	/// Gets or sets the title attribute.
	/// </summary>
	/// <remarks>
	/// Specifies extra information about an element.
	/// </remarks>
	public string Title
	{
		get => this["title"];
		set => this["title"] = value;
	}

	/// <summary>
	/// Gets or sets the translate attribute.
	/// </summary>
	/// <remarks>
	/// HTML5: Specifies whether the content of an element should be translated or not.
	/// </remarks>
	public string Translate
	{
		get => this["translate"];
		set => this["translate"] = value;
	}

	/// <inheritdoc />
	public override int Width => (int) _element.width;

	#endregion

	#region Methods

	/// <inheritdoc />
	public override Element Click(int x = 0, int y = 0, bool refresh = true)
	{
		Browser.ExecuteScript($"document.getElementById(\'{Id}\').click()");
		return this;
	}

	/// <summary>
	/// Fires an event on the element.
	/// </summary>
	/// <param name="eventName"> The events name to fire. </param>
	public void FireEvent(string eventName)
	{
		FireEvent(eventName, new Dictionary<string, string>());
	}

	/// <summary>
	/// Fires an event on the element.
	/// </summary>
	/// <param name="eventName"> The events name to fire. </param>
	/// <param name="eventProperties"> The properties for the event. </param>
	public void FireEvent(string eventName, Dictionary<string, string> eventProperties)
	{
		var values = eventProperties.Aggregate("", (current, item) => current + "{ key: '" + item.Key + "', value: '" + item.Value + "'},");
		if (values.Length > 0)
		{
			values = values.Remove(values.Length - 1, 1);
		}

		var script = $"Speedy.triggerEvent(document.getElementById(\'{Id}\'),{GetFrameIdInsert()},\'{eventName.ToLower()}\',[{values}]);";
		Browser.ExecuteScript(script);
	}

	/// <summary>
	/// Focuses on the element.
	/// </summary>
	public override Element Focus()
	{
		Browser.ExecuteScript($"document.getElementById(\'{Id}\').focus()");
		FireEvent("focus", new Dictionary<string, string>());
		return this;
	}

	/// <summary>
	/// Gets an attribute value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute to read. </param>
	/// <returns> The attribute value. </returns>
	public string GetAttributeValue(string name)
	{
		return GetAttributeValue(name, Browser.AutoRefresh);
	}

	/// <summary>
	/// Gets an attribute value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute to read. </param>
	/// <param name="refresh"> A flag to force the element to refresh. </param>
	/// <returns> The attribute value. </returns>
	public string GetAttributeValue(string name, bool refresh)
	{
		string value;

		if (refresh)
		{
			name = _propertiesToRename.ContainsKey(name) ? _propertiesToRename[name] : name;
			var script = $"Speedy.getElementValue(\'{Id}\',{GetFrameIdInsert()},\'{name}\')";
			value = Browser.ExecuteScript(script);
		}
		else
		{
			value = GetCachedAttribute(name);
		}

		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}

		AddOrUpdateElementAttribute(name, value);
		return value;
	}

	/// <summary>
	/// Gets the HTML displayed in the element.
	/// </summary>
	public string GetHtml()
	{
		return Browser.ExecuteScript("document.getElementById('" + Id + "').innerHTML");
	}

	/// <summary>
	/// Gets an attribute style value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute style to read. </param>
	/// <returns> The attribute style value. </returns>
	public string GetStyleAttributeValue(string name)
	{
		return GetStyleAttributeValue(name, Browser.AutoRefresh);
	}

	/// <summary>
	/// Gets an attribute style value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute style to read. </param>
	/// <param name="forceRefresh"> A flag to force the element to refresh. </param>
	/// <returns> The attribute style value. </returns>
	public string GetStyleAttributeValue(string name, bool forceRefresh)
	{
		var styleValue = GetAttributeValue("style", forceRefresh);
		if (styleValue == null)
		{
			return string.Empty;
		}

		var styleValues = styleValue.Split(';')
			.Select(x => x.Split(':'))
			.Where(x => x.Length == 2)
			.Select(x => new KeyValuePair<string, string>(x[0].Trim(), x[1].Trim()))
			.ToList()
			.ToDictionary(x => x.Key, x => x.Value);

		return styleValues.ContainsKey(name) ? styleValues[name] : string.Empty;
	}

	/// <summary>
	/// Highlight or resets the element.
	/// </summary>
	/// <param name="highlight">
	/// If true the element is highlight yellow. If false the element is returned to its original
	/// color.
	/// </param>
	public void Highlight(bool highlight)
	{
		//LogManager.Write(highlight ? "Adding highlight to element " + Id + "." : "Removing highlight from element " + Id + ".", LogLevel.Verbose);
		SetStyleAttributeValue("background-color", highlight ? _highlightColor : _originalColor);

		if (Browser.Application.SlowMotion && highlight)
		{
			Thread.Sleep(150);
		}
	}

	/// <inheritdoc />
	public override Element LeftClick(int x = 0, int y = 0)
	{
		var clickX = Location.X + x + (Width > 0 ? Width / 2 : 0);
		var clickY = Location.Y + y + (Height > 0 ? Height / 2 : 0);

		Input.Mouse.LeftButtonClick(clickX, clickY);
		return this;
	}

	/// <inheritdoc />
	public override Element MiddleClick(int x = 0, int y = 0)
	{
		var clickX = Location.X + x + (Width > 0 ? Width / 2 : 0);
		var clickY = Location.Y + y + (Height > 0 ? Height / 2 : 0);

		Input.Mouse.MiddleButtonClick(clickX, clickY);
		return this;
	}

	/// <inheritdoc />
	public override Element MoveMouseTo(int x = 0, int y = 0)
	{
		Input.Mouse.MoveTo(Location.X + x, Location.Y + y);
		return this;
	}

	/// <inheritdoc />
	public override ElementHost Refresh<T>(Func<T, bool> condition)
	{
		Children.Clear();
		Children.AddRange(Browser.GetElements(this));
		return this;
	}

	/// <summary>
	/// Remove the element attribute. * Experimental
	/// </summary>
	/// <param name="name"> The name of the attribute. </param>
	public void RemoveAttribute(string name)
	{
		Browser.RemoveElementAttribute(this, name);
	}

	/// <inheritdoc />
	public override Element RightClick(int x = 0, int y = 0)
	{
		var clickX = Location.X + x + (Width > 0 ? Width / 2 : 0);
		var clickY = Location.Y + y + (Height > 0 ? Height / 2 : 0);

		Input.Mouse.RightButtonClick(clickX, clickY);
		return this;
	}

	/// <summary>
	/// Scroll the element into view.
	/// </summary>
	public WebElement ScrollIntoView()
	{
		Browser.ExecuteScript($"$(\'#{Id}\')[0].scrollIntoView()");
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(string text)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(text);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(string text, params KeyboardKey[] keys)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(text, keys);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(string text, TimeSpan delay, params KeyboardKey[] keys)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(text, delay, keys);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(string text, TimeSpan delay, params KeyStroke[] keyStrokes)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(text, delay, keyStrokes);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(params KeyboardKey[] keys)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(keys);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(KeyboardModifier modifiers, params KeyboardKey[] keys)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(modifiers, keys);
		return this;
	}

	/// <inheritdoc />
	public override Element SendInput(params KeyStroke[] keyStrokes)
	{
		Browser.Focus();
		Focus();
		Input.Keyboard.SendInput(keyStrokes);
		return this;
	}

	/// <summary>
	/// Sets an attribute value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute to write. </param>
	/// <param name="value"> The value to be written. </param>
	public void SetAttributeValue(string name, string value)
	{
		name = _propertiesToRename.ContainsKey(name) ? _propertiesToRename[name] : name;
		value = Browser.CleanupScriptForJavascriptString(value);

		var script = $"Speedy.setElementValue(\'{Id}\',{GetFrameIdInsert()},\'{name}\',\'{value}\')";
		Browser.ExecuteScript(script);
		AddOrUpdateElementAttribute(name, value);
		TriggerElement();
	}

	/// <summary>
	/// Sets the HTML displayed in the element.
	/// </summary>
	public void SetHtml(string html)
	{
		html = Browser.CleanupScriptForJavascriptString(html);
		Browser.ExecuteScript($"document.getElementById(\'{Id}\').innerHTML = \'{html}\'", false);
	}

	/// <summary>
	/// Sets an attribute style value by the provided name.
	/// </summary>
	/// <param name="name"> The name of the attribute style to write. </param>
	/// <param name="value"> The style value to be written. </param>
	public void SetStyleAttributeValue(string name, string value)
	{
		var styleValue = GetCachedAttribute("style") ?? string.Empty;
		var styleValues = styleValue
			.Split(';')
			.Select(x => x.Split(':'))
			.Where(x => x.Length == 2)
			.Select(x => new KeyValuePair<string, string>(x[0], x[1]))
			.ToList()
			.ToDictionary(x => x.Key, x => x.Value);

		if (!styleValues.ContainsKey(name))
		{
			styleValues.Add(name, value);
		}

		if (string.IsNullOrWhiteSpace(value))
		{
			styleValues.Remove(name);
		}
		else
		{
			styleValues[name] = value;
		}

		styleValue = string.Join(";", styleValues.Select(x => x.Key + ":" + x.Value));
		SetAttributeValue("style", styleValue);
	}

	/// <summary>
	/// Provides a string of details for the element.
	/// </summary>
	/// <returns> The string of element details. </returns>
	public override string ToDetailString()
	{
		var builder = new StringBuilder();

		builder.AppendLine("id : " + Id);
		builder.AppendLine("name : " + Name);
		builder.AppendLine("type : " + GetType().Name);

		for (var i = 0; i < _element.attributes.Count; i++)
		{
			string attributeName = _element.attributes[i++].ToString();
			builder.AppendLine($"{attributeName} : {_element.attributes[i]}");
		}

		return builder.ToString();
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{GetType().Name} : {Id} : {Name}";
	}

	/// <inheritdoc />
	public override ElementHost WaitForComplete(int minimumDelay = 0)
	{
		return Browser.WaitForComplete(minimumDelay);
	}

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
	}

	/// <summary>
	/// First the key code event properties for the character.
	/// </summary>
	/// <param name="character"> The character to get the event properties for. </param>
	/// <returns> An event properties for the character. </returns>
	protected Dictionary<string, string> GetKeyCodeEventProperty(char character)
	{
		return new Dictionary<string, string>
		{
			{ "keyCode", ((int) character).ToString() },
			{ "charCode", ((int) character).ToString() },
			{ "which", ((int) character).ToString() }
		};
	}

	/// <summary>
	/// Triggers the element to ensure the element detect changes.
	/// </summary>
	protected void TriggerElement()
	{
		if (Browser.JavascriptLibraries.Contains(JavaScriptLibrary.Angular))
		{
			Browser.ExecuteScript("angular.element(document.querySelector('#" + Id + "')).triggerHandler('input');", false);
			Browser.ExecuteScript("angular.element(document.querySelector('#" + Id + "')).trigger('input');", false);
			Browser.ExecuteScript("angular.element(document.querySelector('#" + Id + "')).trigger('change');", false);
		}

		if (Browser.JavascriptLibraries.Contains(JavaScriptLibrary.Vue))
		{
			FireEvent("input");
			FireEvent("change");
		}
	}

	/// <summary>
	/// Adds a collection of elements and initializes them as their specific element type.
	/// </summary>
	/// <param name="token"> The collection of elements to add. </param>
	/// <param name="browser"> The browser the element is for. </param>
	/// <param name="parent"> The parent for this element. </param>
	internal static WebElement Create(JToken token, Browser browser, ElementHost parent)
	{
		var element = new WebElement(token, browser, parent);

		switch (element.TagName)
		{
			case "a":
				return new Link(token, browser, parent);

			case "abbr":
				return new Abbreviation(token, browser, parent);

			case "acronym":
				return new Acronym(token, browser, parent);

			case "address":
				return new Address(token, browser, parent);

			case "applet":
				return new Applet(token, browser, parent);

			case "area":
				return new Area(token, browser, parent);

			case "article":
				return new Article(token, browser, parent);

			case "aside":
				return new Aside(token, browser, parent);

			case "audio":
				return new Audio(token, browser, parent);

			case "b":
				return new Bold(token, browser, parent);

			case "base":
				return new Base(token, browser, parent);

			case "basefont":
				return new BaseFont(token, browser, parent);

			case "bdi":
				return new BiDirectionalIsolation(token, browser, parent);

			case "bdo":
				return new BiDirectionalOverride(token, browser, parent);

			case "big":
				return new Big(token, browser, parent);

			case "blockquote":
				return new BlockQuote(token, browser, parent);

			case "body":
				return new Body(token, browser, parent);

			case "br":
				return new LineBreak(token, browser, parent);

			case "button":
				return new Button(token, browser, parent);

			case "canvas":
				return new Canvas(token, browser, parent);

			case "caption":
				return new Caption(token, browser, parent);

			case "center":
				return new Center(token, browser, parent);

			case "cite":
				return new Cite(token, browser, parent);

			case "code":
				return new Code(token, browser, parent);

			case "col":
				return new Column(token, browser, parent);

			case "colgroup":
				return new ColumnGroup(token, browser, parent);

			case "datalist":
				return new DataList(token, browser, parent);

			case "dd":
				return new DescriptionListDefinition(token, browser, parent);

			case "del":
				return new Deleted(token, browser, parent);

			case "details":
				return new Details(token, browser, parent);

			case "dfn":
				return new Definition(token, browser, parent);

			case "dialog":
				return new Dialog(token, browser, parent);

			case "dir":
				return new Directory(token, browser, parent);

			case "div":
				return new Division(token, browser, parent);

			case "dl":
				return new DescriptionList(token, browser, parent);

			case "dt":
				return new DescriptionListTerm(token, browser, parent);

			case "em":
				return new Emphasis(token, browser, parent);

			case "embed":
				return new Embed(token, browser, parent);

			case "fieldset":
				return new FieldSet(token, browser, parent);

			case "figcaption":
				return new FigureCaption(token, browser, parent);

			case "figure":
				return new Figure(token, browser, parent);

			case "font":
				return new Font(token, browser, parent);

			case "footer":
				return new Footer(token, browser, parent);

			case "form":
				return new Form(token, browser, parent);

			case "frame":
				return new Frame(token, browser, parent);

			case "frameset":
				return new FrameSet(token, browser, parent);

			case "head":
				return new Head(token, browser, parent);

			case "header":
				return new Header(token, browser, parent);

			case "hgroup":
				return new HeadingGroup(token, browser, parent);

			case "h1":
			case "h2":
			case "h3":
			case "h4":
			case "h5":
			case "h6":
				return new Header(token, browser, parent);

			case "hr":
				return new HorizontalRule(token, browser, parent);

			case "html":
				return new Html(token, browser, parent);

			case "i":
				return new Italic(token, browser, parent);

			case "iframe":
				return new InlineFrame(token, browser, parent);

			case "img":
				return new Image(token, browser, parent);

			case "input":
				var type = element.GetAttributeValue("type", false).ToLower();
				switch (type)
				{
					case "checkbox":
						return new CheckBox(token, browser, parent);

					case "image":
						return new Image(token, browser, parent);

					case "button":
					case "submit":
					case "reset":
						return new Button(token, browser, parent);

					case "email":
					case "hidden":
					case "number":
					case "password":
					case "search":
					case "tel":
					case "text":
					case "url":
						return new TextInput(token, browser, parent);

					case "radio":
						return new RadioButton(token, browser, parent);

					default:
						return element;
				}

			case "ins":
				return new Insert(token, browser, parent);

			case "kbd":
				return new Keyboard(token, browser, parent);

			case "keygen":
				return new KeyGenerator(token, browser, parent);

			case "label":
				return new Label(token, browser, parent);

			case "legend":
				return new Legend(token, browser, parent);

			case "li":
				return new ListItem(token, browser, parent);

			case "link":
				return new StyleSheetLink(token, browser, parent);

			case "main":
				return new Main(token, browser, parent);

			case "map":
				return new Map(token, browser, parent);

			case "mark":
				return new Mark(token, browser, parent);

			case "menu":
				return new Menu(token, browser, parent);

			case "menuitem":
				return new MenuItem(token, browser, parent);

			case "meta":
				return new Metadata(token, browser, parent);

			case "meter":
				return new Meter(token, browser, parent);

			case "nav":
				return new Navigation(token, browser, parent);

			case "noframes":
				return new NoFrames(token, browser, parent);

			case "noscript":
				return new NoScript(token, browser, parent);

			case "object":
				return new Object(token, browser, parent);

			case "ol":
				return new OrderedList(token, browser, parent);

			case "optgroup":
				return new OptionGroup(token, browser, parent);

			case "option":
				return new Option(token, browser, parent);

			case "output":
				return new Output(token, browser, parent);

			case "p":
				return new Paragraph(token, browser, parent);

			case "param":
				return new Parameter(token, browser, parent);

			case "pre":
				return new PreformattedText(token, browser, parent);

			case "progress":
				return new Progress(token, browser, parent);

			case "q":
				return new Quotation(token, browser, parent);

			case "rp":
				return new RubyExplanation(token, browser, parent);

			case "rt":
				return new RubyTag(token, browser, parent);

			case "ruby":
				return new Ruby(token, browser, parent);

			case "s":
				return new StrikeThrough(token, browser, parent);

			case "samp":
				return new Sample(token, browser, parent);

			case "script":
				return new Script(token, browser, parent);

			case "section":
				return new Section(token, browser, parent);

			case "select":
				return new Select(token, browser, parent);

			case "small":
				return new Small(token, browser, parent);

			case "source":
				return new Source(token, browser, parent);

			case "span":
				return new Span(token, browser, parent);

			case "strike":
				return new Strike(token, browser, parent);

			case "strong":
				return new Strong(token, browser, parent);

			case "style":
				return new Style(token, browser, parent);

			case "sub":
				return new SubScript(token, browser, parent);

			case "table":
				return new Table(token, browser, parent);

			case "tbody":
				return new TableBody(token, browser, parent);

			case "td":
				return new TableColumn(token, browser, parent);

			case "textarea":
				return new TextArea(token, browser, parent);

			case "tfoot":
				return new TableFooter(token, browser, parent);

			case "th":
				return new TableHeaderColumn(token, browser, parent);

			case "thead":
				return new TableHead(token, browser, parent);

			case "time":
				return new Time(token, browser, parent);

			case "title":
				return new Title(token, browser, parent);

			case "tr":
				return new TableRow(token, browser, parent);

			case "track":
				return new Track(token, browser, parent);

			case "tt":
				return new TeletypeText(token, browser, parent);

			case "u":
				return new Underline(token, browser, parent);

			case "ul":
				return new UnorderedList(token, browser, parent);

			case "var":
				return new Variable(token, browser, parent);

			case "video":
				return new Video(token, browser, parent);

			case "wbr":
				return new WordBreakOpportunity(token, browser, parent);

			default:
				return element;
		}
	}

	internal string GetFrameIdInsert()
	{
		return FrameId == null ? "undefined" : $"\'{FrameId}\'";
	}

	/// <summary>
	/// Add or updates the cached attributes for this element.
	/// </summary>
	/// <param name="name"> </param>
	/// <param name="value"> </param>
	private void AddOrUpdateElementAttribute(string name, string value)
	{
		for (var i = 0; i < _element.attributes.Count; i++)
		{
			string attributeName = _element.attributes[i++].ToString();
			if (attributeName == name)
			{
				_element.attributes[i] = value;
			}
		}

		_element.attributes.Add(name);
		_element.attributes.Add(value);
	}

	/// <summary>
	/// Gets the attribute from the local cache.
	/// </summary>
	/// <param name="name"> The name of the attribute. </param>
	/// <returns> Returns the value or null if the attribute was not found. </returns>
	private string GetCachedAttribute(string name)
	{
		for (var i = 0; i < _element.attributes.Count; i++)
		{
			string attributeName = _element.attributes[i++].ToString();
			if (attributeName == name)
			{
				return _element.attributes[i].ToString();
			}
		}

		return null;
	}

	#endregion
}