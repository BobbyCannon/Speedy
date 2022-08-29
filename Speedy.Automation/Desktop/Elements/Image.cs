#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a image element.
	/// </summary>
	public class Image : DesktopElement
	{
		#region Constructors

		internal Image(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion
	}
}