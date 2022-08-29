#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a document element.
	/// </summary>
	public class Document : ScrollableDesktopElement
	{
		#region Constructors

		internal Document(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the text value.
		/// </summary>
		public string Text
		{
			get => GetText();
			set => SetText(value);
		}

		#endregion
	}
}