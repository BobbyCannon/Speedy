#region References

using System.Linq;
using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Automation.Web.Elements
{
	/// <summary>
	/// Represents a browser select element.
	/// </summary>
	public class Select : WebElement
	{
		#region Constructors

		/// <summary>
		/// Initializes an instance of a browser element.
		/// </summary>
		/// <param name="element"> The browser element this is for. </param>
		/// <param name="browser"> The browser this element is associated with. </param>
		/// <param name="parent"> The parent host for this element. </param>
		public Select(JToken element, Browser browser, ElementHost parent)
			: base(element, browser, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the autofocus attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that the drop-down list should automatically get focus when the page loads.
		/// </remarks>
		public string AutoFocus
		{
			get => this["autofocus"];
			set => this["autofocus"] = value;
		}

		/// <summary>
		/// Gets or sets the disabled attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that a drop-down list should be disabled.
		/// </remarks>
		public string Disabled
		{
			get => this["disabled"];
			set => this["disabled"] = value;
		}

		/// <summary>
		/// Gets or sets the form id (form) attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Defines one or more forms the select field belongs to.
		/// </remarks>
		public string Form
		{
			get => this["form"];
			set => this["form"] = value;
		}

		/// <summary>
		/// Gets or sets the multiple attribute.
		/// </summary>
		/// <remarks>
		/// Specifies that multiple options can be selected at once.
		/// </remarks>
		public string Multiple
		{
			get => this["multiple"];
			set => this["multiple"] = value;
		}

		/// <summary>
		/// Gets or sets the size attribute.
		/// </summary>
		/// <remarks>
		/// Defines the number of visible options in a drop-down list.
		/// </remarks>
		public string OptionCount
		{
			get => this["size"];
			set => this["size"] = value;
		}

		/// <summary>
		/// Gets or sets the required attribute.
		/// </summary>
		/// <remarks>
		/// HTML5: Specifies that the user is required to select a value before submitting the form.
		/// </remarks>
		public string Required
		{
			get => this["required"];
			set => this["required"] = value;
		}

		/// <summary>
		/// Returns the selected option or null if nothing is selected.
		/// </summary>
		public Option SelectedOption
		{
			get { return Children.OfType<Option>().FirstOrDefault(x => x.Value == Value); }
		}

		/// <inheritdoc />
		public override string Text
		{
			get => Browser.ExecuteScript($"Speedy.getSelectText(\'{Id}\',{GetFrameIdInsert()})");
			set => Browser.ExecuteScript($"Speedy.setSelectText(\'{Id}\',{GetFrameIdInsert()},\'{value}\')");
		}

		/// <summary>
		/// Gets or sets the value for this select.
		/// </summary>
		public string Value
		{
			get => this["value"];
			set => this["value"] = value;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override Element SendInput(string value)
		{
			try
			{
				Click();
				Focus();
				Highlight(true);
				Text = value;
				return this;
			}
			finally
			{
				Highlight(false);
			}
		}

		#endregion
	}
}