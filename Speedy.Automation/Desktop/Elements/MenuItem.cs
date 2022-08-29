#region References

using Interop.UIAutomationClient;
using Speedy.Automation.Desktop.Pattern;

#endregion

namespace Speedy.Automation.Desktop.Elements
{
	/// <summary>
	/// Represents the menu item for a window.
	/// </summary>
	public class MenuItem : DesktopElement
	{
		#region Constructors

		internal MenuItem(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the menu expand collapse state.
		/// </summary>
		public bool IsExpanded => ExpandCollapsePattern.Create(this)?.IsExpanded ?? false;

		/// <summary>
		/// Gets a value indicating whether this menu item supports expanding and collapsing pattern.
		/// </summary>
		public bool SupportsExpandingCollapsing => ExpandCollapsePattern.Create(this) != null;

		#endregion

		#region Methods

		/// <summary>
		/// Performs mouse click at the center of the element.
		/// </summary>
		/// <param name="x"> Optional X offset when clicking. </param>
		/// <param name="y"> Optional Y offset when clicking. </param>
		/// <param name="refresh"> Optional value to refresh the element's children. </param>
		public override Element Click(int x = 0, int y = 0, bool refresh = true)
		{
			base.Click(x, y);

			if (refresh)
			{
				Refresh();
			}

			return this;
		}

		/// <summary>
		/// Collapse the menu item.
		/// </summary>
		public MenuItem Collapse()
		{
			ExpandCollapsePattern.Create(this)?.Collapse();
			return this;
		}

		/// <summary>
		/// Expand the menu item.
		/// </summary>
		public MenuItem Expand()
		{
			ExpandCollapsePattern.Create(this)?.Expand();
			return this;
		}

		#endregion
	}
}