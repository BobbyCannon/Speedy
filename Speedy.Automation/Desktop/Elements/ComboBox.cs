#region References

using Interop.UIAutomationClient;
using Speedy.Automation.Desktop.Pattern;
using ExpandCollapseState = Speedy.Automation.Desktop.Pattern.ExpandCollapseState;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents a combo box element.
	/// </summary>
	public class ComboBox : DesktopElement
	{
		#region Constructors

		internal ComboBox(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current expanded state of the combo box.
		/// </summary>
		public ExpandCollapseState ExpandCollapseState => ExpandCollapsePattern.Create(this)?.ExpandCollapseState ?? ExpandCollapseState.Collapsed;

		/// <summary>
		/// Gets a value indicating whether the control can have a value set programmatically, or that can be edited by the user.
		/// </summary>
		public bool ReadOnly => ValuePattern.Create(this)?.IsReadOnly ?? true;

		/// <summary>
		/// Gets the text value.
		/// </summary>
		public string Text => ValuePattern.Create(this)?.Value;

		#endregion

		#region Methods

		/// <summary>
		/// Collapses the combo box.
		/// </summary>
		public void Collapse()
		{
			ExpandCollapsePattern.Create(this)?.Collapse();
		}

		/// <summary>
		/// Expands the combo box.
		/// </summary>
		public void Expand()
		{
			ExpandCollapsePattern.Create(this)?.Expand();
		}

		#endregion
	}
}