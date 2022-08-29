#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represent a browser form element.
	/// </summary>
	public class Form : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a form browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Form(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the action attribute.
		/// </summary>
		/// <remarks>
		/// Specifies where to send the form-data when a form is submitted.
		/// </remarks>
		public string Action
		{
			get => this["action"];
			set => this["action"] = value;
		}

		/// <summary>
		/// Gets or sets the auto complete attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies whether a form should have autocomplete on or off.
		/// </remarks>
		public string AutoComplete
		{
			get => this["autocomplete"];
			set => this["autocomplete"] = value;
		}

		/// <summary>
		/// Gets or sets the encoded type attribute.
		/// </summary>
		/// <remarks>
		/// Specifies how the form-data should be encoded when submitting it to the server (only for method="post").
		/// </remarks>
		public string EncType
		{
			get => this["enctype"];
			set => this["enctype"] = value;
		}

		/// <summary>
		/// Gets or sets the method attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the HTTP method to use when sending form-data.
		/// </remarks>
		public string Method
		{
			get => this["method"];
			set => this["method"] = value;
		}

		/// <summary>
		/// Gets or sets the no validate attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that the form should not be validated when submitted.
		/// </remarks>
		public string NoValidate
		{
			get => this["novalidate"];
			set => this["novalidate"] = value;
		}

		/// <summary>
		/// Gets or sets the target attribute.
		/// </summary>
		/// <remarks>
		/// Specifies where to display the response that is received after submitting the form.
		/// </remarks>
		public string Target
		{
			get => this["target"];
			set => this["target"] = value;
		}

		#endregion
	}
}