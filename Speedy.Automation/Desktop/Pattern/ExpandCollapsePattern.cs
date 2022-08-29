#region References

using System.Linq;
using Interop.UIAutomationClient;
using Speedy.Automation.Internal;

#endregion

namespace Speedy.Automation.Desktop.Pattern
{
	/// <summary>
	/// Represents the Windows expand / collapse pattern.
	/// </summary>
	public class ExpandCollapsePattern
	{
		#region Fields

		private readonly IUIAutomationExpandCollapsePattern _pattern;

		#endregion

		#region Constructors

		private ExpandCollapsePattern(IUIAutomationExpandCollapsePattern pattern)
		{
			_pattern = pattern;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current expanded state of the element.
		/// </summary>
		public ExpandCollapseState ExpandCollapseState => _pattern.CurrentExpandCollapseState.Convert();

		/// <summary>
		/// Gets the value indicating the element is expanded.
		/// </summary>
		public bool IsExpanded
		{
			get
			{
				var expandedStates = new[] { ExpandCollapseState.Expanded, ExpandCollapseState.PartiallyExpanded };
				return expandedStates.Contains(_pattern.CurrentExpandCollapseState.Convert());
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Collapses the element.
		/// </summary>
		public void Collapse()
		{
			_pattern.Collapse();
		}

		/// <summary>
		/// Create a new pattern for the provided element.
		/// </summary>
		/// <param name="element"> The element that supports the pattern. </param>
		/// <returns> The pattern if we could find one else null will be returned. </returns>
		public static ExpandCollapsePattern Create(DesktopElement element)
		{
			var pattern = element.NativeElement.GetCurrentPattern(UIA_PatternIds.UIA_ExpandCollapsePatternId) as IUIAutomationExpandCollapsePattern;
			return pattern == null ? null : new ExpandCollapsePattern(pattern);
		}

		/// <summary>
		/// Expands the element.
		/// </summary>
		public void Expand()
		{
			_pattern.Expand();
		}

		#endregion
	}
}