#region References

using Newtonsoft.Json.Linq;
using Speedy.Automation.Internal;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser Applet element.
	/// </summary>
	public class Applet : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Applet(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the code attribute. This should be a URL to the applet class file.
		/// </summary>
		/// <remarks>
		/// Specifies the file name of a Java applet.
		/// </remarks>
		public string Code
		{
			get => this["code"];
			set => this["code"] = value;
		}

		/// <summary>
		/// Gets or sets the height attribute.
		/// </summary>
		public override int Height => this["height"].ToInt();

		/// <summary>
		/// Gets or sets the object attribute.
		/// </summary>
		/// <remarks>
		/// Specifies a reference to a serialized representation of an applet.
		/// </remarks>
		public string Object
		{
			get => this["object"];
			set => this["object"] = value;
		}

		/// <summary>
		/// Gets or sets the width attribute.
		/// </summary>
		public override int Width => this["width"].ToInt();

		#endregion
	}
}