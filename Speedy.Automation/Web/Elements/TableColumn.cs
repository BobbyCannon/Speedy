#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser table column (td) element.
	/// </summary>
	public class TableColumn : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public TableColumn(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the column span (colspan) attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the number of columns a cell should span.
		/// </remarks>
		public string ColumnSpan
		{
			get => this["colspan"];
			set => this["colspan"] = value;
		}

		/// <summary>
		/// Gets or sets the header id (headers) attribute.
		/// </summary>
		/// <remarks>
		/// Specifies one or more header cells a cell is related to.
		/// </remarks>
		public string Headers
		{
			get => this["headers"];
			set => this["headers"] = value;
		}

		/// <summary>
		/// Gets or sets the row span (rowspan) attribute.
		/// </summary>
		/// <remarks>
		/// Sets the number of rows a cell should span.
		/// </remarks>
		public string RowSpan
		{
			get => this["rowspan"];
			set => this["rowspan"] = value;
		}

		#endregion
	}
}