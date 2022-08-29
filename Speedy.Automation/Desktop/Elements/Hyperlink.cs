#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a hyperlink element.
	/// </summary>
	public class Hyperlink : DesktopElement
	{
		#region Constructors

		internal Hyperlink(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion
	}
}