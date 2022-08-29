#region References

using Newtonsoft.Json.Linq;
using Speedy.Automation.Internal;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser text input["image"] element.
	/// </summary>
	public class Image : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a Image browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Image(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the alt attribute.
		/// </summary>
		/// <remarks>
		/// Specifies an alternate text for images.
		/// </remarks>
		public string Alt
		{
			get => this["alt"];
			set => this["alt"] = value;
		}

		/// <summary>
		/// Gets or sets the height attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies the height of an input element.
		/// </remarks>
		public override int Height => this["height"].ToInt();

		/// <summary>
		/// Gets or sets the value attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the URL of the image to use as a submit button.
		/// </remarks>
		public string Src
		{
			get => this["src"];
			set => this["src"] = value;
		}

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
		/// Gets or sets the width attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies the width of an input element.
		/// </remarks>
		public override int Width => this["width"].ToInt();

		#endregion
	}
}