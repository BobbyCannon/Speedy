#region References

using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represent a browser link element.
	/// </summary>
	public class Link : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a Link browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Link(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or set the download attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that the target will be downloaded when a user clicks on the hyper link.
		/// </remarks>
		public string Download
		{
			get => this["download"];
			set => this["download"] = value;
		}

		/// <summary>
		/// Gets or set the hypertext reference (href) attribute.
		/// </summary>
		/// <remarks>
		/// Specifies the URL of the page the link goes to.
		/// </remarks>
		public string Href
		{
			get => this["href"];
			set => this["href"] = value;
		}

		/// <summary>
		/// Gets or set the media attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies what media/device the linked document is optimized for.
		/// </remarks>
		public string Media
		{
			get => this["media"];
			set => this["media"] = value;
		}

		/// <summary>
		/// Gets or set the hypertext reference of this link.
		/// </summary>
		/// <remarks>
		/// The rel attribute specifies the relationship between the current document and the linked
		/// document. Only used if the href attribute is present.
		/// </remarks>
		public string Rel
		{
			get => this["rel"];
			set => this["rel"] = value;
		}

		/// <summary>
		/// Gets or set the target of this link.
		/// </summary>
		/// <remarks>
		/// Specifies where to open the linked document.
		/// </remarks>
		public string Target
		{
			get => this["target"];
			set => this["target"] = value;
		}

		/// <summary>
		/// Gets or set the media type of this link.
		/// </summary>
		/// <remarks>
		/// The Internet media type of the linked document. Look at IANA Media Types for a complete
		/// list of standard media types.
		/// </remarks>
		public string Type
		{
			get => this["type"];
			set => this["type"] = value;
		}

		#endregion
	}
}