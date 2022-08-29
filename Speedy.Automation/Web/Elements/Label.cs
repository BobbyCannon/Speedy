#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser label element.
	/// </summary>
	public class Label : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Label(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the element id (for) attribute.
		/// </summary>
		/// <remarks>
		/// Specifies which form element a label is bound to.
		/// </remarks>
		public string For
		{
			get => this["for"];
			set => this["for"] = value;
		}

		/// <summary>
		/// Gets or sets the form id (form) attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies one or more forms the label belongs to.
		/// </remarks>
		public string Form
		{
			get => this["form"];
			set => this["form"] = value;
		}

		#endregion
	}
}