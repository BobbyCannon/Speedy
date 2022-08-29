#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser option element.
	/// </summary>
	public class Option : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Option(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the disabled attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that an option should be disabled.
		/// </remarks>
		public string Disabled
		{
			get => this["disabled"];
			set => this["disabled"] = value;
		}

		/// <summary>
		/// Gets or sets the label attribute.
		/// </summary>
		/// <remarks>
		/// Specifies a shorter label for an option.
		/// </remarks>
		public string Label
		{
			get => this["label"];
			set => this["label"] = value;
		}

		/// <summary>
		/// Gets or sets the selected attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that an option should be pre-selected when the page loads.
		/// </remarks>
		public string Selected
		{
			get => this["selected"];
			set => this["selected"] = value;
		}

		/// <summary>
		/// Gets or sets the value for this select.
		/// </summary>
		/// <remarks>
		/// Specifies the value to be sent to a server.
		/// </remarks>
		public string Value
		{
			get => this["value"];
			set
			{
				this["value"] = value;
				TriggerElement();
			}
		}

		#endregion
	}
}