#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a radio button element.
	/// </summary>
	public class RadioButton : DesktopElement
	{
		#region Constructors

		internal RadioButton(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion
	}
}