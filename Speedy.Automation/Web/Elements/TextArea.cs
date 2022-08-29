#region References

using System;
using System.Threading;
using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser text area element.
	/// </summary>
	public class TextArea : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a text area browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public TextArea(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
			TypingDelay = Browser.Application.SlowMotion ? 50 : 0;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the autofocus attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that an input element should automatically get focus when the page loads.
		/// </remarks>
		public string AutoFocus
		{
			get => this["autofocus"];
			set => this["autofocus"] = value;
		}

		/// <summary>
		/// Gets or sets the cols attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the visible width of a text area.
		/// </remarks>
		public string Cols
		{
			get => this["cols"];
			set => this["cols"] = value;
		}

		/// <summary>
		/// Gets or sets the disabled attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that a button should be disabled.
		/// </remarks>
		public string Disabled
		{
			get => this["disabled"];
			set => this["disabled"] = value;
		}

		/// <summary>
		/// Gets or sets the form attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies one or more forms the text area belongs to.
		/// </remarks>
		public string Form
		{
			get => this["form"];
			set => this["form"] = value;
		}

		/// <summary>
		/// Gets or sets the max length attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies the maximum number of characters allowed in the text area.
		/// </remarks>
		public string MaxLength
		{
			get => this["maxlength"];
			set => this["maxlength"] = value;
		}

		/// <summary>
		/// Gets or sets the place holder attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies a short hint that describes the expected value of an input element.
		/// </remarks>
		public string PlaceHolder
		{
			get => this["placeholder"];
			set => this["placeholder"] = value;
		}

		/// <summary>
		/// Gets or sets the read only attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that an input field is read-only
		/// </remarks>
		public string ReadOnly
		{
			get => this["readonly"];
			set => this["readonly"] = value;
		}

		/// <summary>
		/// Gets or sets the required attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that a text area is required/must be filled out.
		/// </remarks>
		public string Required
		{
			get => this["required"];
			set => this["required"] = value;
		}

		/// <summary>
		/// Gets or sets the rows attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the visible number of lines in a text area.
		/// </remarks>
		public string Rows
		{
			get => this["rows"];
			set => this["rows"] = value;
		}

		/// <summary>
		/// Gets or sets the value attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the value of an input element.
		/// </remarks>
		public override string Text
		{
			get => this["value"];
			set => SendInput(value, true);
		}

		/// <summary>
		/// Gets the delay (in milliseconds) between each character.
		/// </summary>
		public int TypingDelay { get; set; }

		/// <summary>
		/// Gets or sets the value attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the value of an input element.
		/// </remarks>
		public string Value
		{
			get => this["value"];
			set => this["value"] = value;
		}

		/// <summary>
		/// Gets or sets the wrap attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies how the text in a text area is to be wrapped when submitted in a form.
		/// </remarks>
		public string Wrap
		{
			get => this["wrap"];
			set => this["wrap"] = value;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override Element SendInput(string value)
		{
			return SendInput(value, false);
		}

		/// <summary>
		/// Set the text of the element to the value provided.
		/// </summary>
		/// <param name="text"> The text to set as the value. </param>
		/// <param name="reset"> Clear the input before setting the text. </param>
		public Element SendInput(string text, bool reset)
		{
			Click();
			Focus();
			Highlight(true);

			var value = reset ? text : GetAttributeValue("value") + text;
			SetAttributeValue("value", value);
			Thread.Sleep(TypingDelay);

			Highlight(false);
			TriggerElement();
			return this;
		}

		/// <inheritdoc />
		public override Element SendInput(string text, TimeSpan delay)
		{
			Click();
			Focus();
			Highlight(true);

			SetAttributeValue("value", text);
			Thread.Sleep(TypingDelay);

			Highlight(false);
			TriggerElement();
			return this;
		}

		/// <summary>
		/// Type text into the element.
		/// </summary>
		/// <param name="value"> The value to be typed. </param>
		/// <param name="reset"> Resets the text in the element before typing the text. </param>
		public Element SendKeys(string value, bool reset)
		{
			Click();
			Focus();
			Highlight(true);

			var newValue = reset ? string.Empty : Text;

			foreach (var character in value)
			{
				var eventProperty = GetKeyCodeEventProperty(character);
				newValue += character;
				FireEvent("keyDown", eventProperty);
				SetAttributeValue("value", newValue);
				FireEvent("keyPress", eventProperty);
				FireEvent("keyUp", eventProperty);
				Thread.Sleep(TypingDelay);
			}

			Thread.Sleep(TypingDelay);

			Highlight(false);
			TriggerElement();
			return this;
		}

		#endregion
	}
}