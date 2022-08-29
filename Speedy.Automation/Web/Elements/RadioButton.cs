#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represent a browser input radio button element.
	/// </summary>
	public class RadioButton : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a input radio button browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public RadioButton(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the checked attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that an element should be pre-selected when the page loads (for type="checkbox" or type="radio").
		/// </remarks>
		public bool Checked
		{
			get => this["checked"] == "true";
			set => this["checked"] = value.ToString();
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
		/// Gets or sets the value attribute.
		/// </summary>
		public override string Text
		{
			get => TagName == "button" ? this["value"] : this["textContent"];
			set => this[TagName == "button" ? "value" : "textContent"] = value;
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