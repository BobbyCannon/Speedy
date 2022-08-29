#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser WordBreakOpportunity (wbr) element.
	/// </summary>
	public class WordBreakOpportunity : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public WordBreakOpportunity(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion
	}
}