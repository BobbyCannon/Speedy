#region References

using Interop.UIAutomationClient;
using Speedy.Automation.Desktop.Pattern;
using ToggleState = Speedy.Automation.Desktop.Pattern.ToggleState;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a check box element.
	/// </summary>
	public class CheckBox : DesktopElement
	{
		#region Constructors

		internal CheckBox(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a flag indicating if the checkbox is checked.
		/// </summary>
		public bool Checked => TogglePattern.Create(this)?.ToggleState != ToggleState.Off;

		/// <summary>
		/// Gets the state of the checkbox.
		/// </summary>
		public ToggleState CheckedState => TogglePattern.Create(this)?.ToggleState ?? ToggleState.Indeterminate;

		/// <summary>
		/// Gets a value indicating whether the control can have a value set programmatically, or that can be edited by the user.
		/// </summary>
		public bool ReadOnly => ValuePattern.Create(this)?.IsReadOnly ?? true;

		#endregion

		#region Methods

		/// <summary>
		/// Toggle the checkbox.
		/// </summary>
		public CheckBox Toggle()
		{
			TogglePattern.Create(this)?.Toggle();
			return this;
		}

		#endregion
	}
}