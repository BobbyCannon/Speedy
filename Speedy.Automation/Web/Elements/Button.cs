#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represent a browser button element.
	/// </summary>
	public class Button : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a Button browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Button(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the autofocus attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that a button should automatically get focus when the page loads.
		/// </remarks>
		public string AutoFocus
		{
			get => this["autofocus"];
			set => this["autofocus"] = value;
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
		/// HTML5: Specifies one or more forms the button belongs to.
		/// </remarks>
		public string Form
		{
			get => this["form"];
			set => this["form"] = value;
		}

		/// <summary>
		/// Gets or sets the form action attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies where to send the form-data when a form is submitted. Only for type="submit".
		/// </remarks>
		public string FormAction
		{
			get => this["formaction"];
			set => this["formaction"] = value;
		}

		/// <summary>
		/// Gets or sets the form encoded type attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies how form-data should be encoded before sending it to a server. Only for type="submit".
		/// </remarks>
		public string FormEncType
		{
			get => this["formenctype"];
			set => this["formenctype"] = value;
		}

		/// <summary>
		/// Gets or sets the form method attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies how to send the form-data (which HTTP method to use). Only for type="submit".
		/// </remarks>
		public string FormMethod
		{
			get => this["formmethod"];
			set => this["formmethod"] = value;
		}

		/// <summary>
		/// Gets or sets the form no validate attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that the form-data should not be validated on submission. Only for type="submit".
		/// </remarks>
		public string FormNoValidate
		{
			get => this["formnovalidate"];
			set => this["formnovalidate"] = value;
		}

		/// <summary>
		/// Gets or sets the form target attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies where to display the response after submitting the form. Only for type="submit".
		/// </remarks>
		public string FormTarget
		{
			get => this["formtarget"];
			set => this["formtarget"] = value;
		}

		/// <summary>
		/// Gets or sets the value attribute.
		/// </summary>
		public override string Text
		{
			get => TagName == "input" ? this["value"] : this["textContent"];
			set => this[TagName == "input" ? "value" : "textContent"] = value;
		}

		/// <summary>
		/// Gets or sets the value attribute.
		/// </summary>
		public string Value
		{
			get => this["value"];
			set => this["value"] = value;
		}

		#endregion
	}
}