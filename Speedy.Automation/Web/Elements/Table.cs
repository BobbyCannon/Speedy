#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser table (table) element.
	/// </summary>
	public class Table : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Table(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the sortable attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that the table should be sortable.
		/// </remarks>
		public string Sortable
		{
			get => this["sortable"];
			set => this["sortable"] = value;
		}

		#endregion
	}
}