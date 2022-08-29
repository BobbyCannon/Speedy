#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Pattern
{
	/// <summary>
	/// Represents the Windows scroll pattern.
	/// </summary>
	public class ScrollPattern
	{
		#region Fields

		private readonly IUIAutomationScrollPattern _pattern;

		#endregion

		#region Constructors

		private ScrollPattern(IUIAutomationScrollPattern pattern)
		{
			_pattern = pattern;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the scroll percentage for the horizontal bar.
		/// </summary>
		public double HorizontalScrollPercent => _pattern?.CurrentHorizontalScrollPercent ?? 0;

		/// <summary>
		/// Gets the scroll percentage for the vertical bar.
		/// </summary>
		public double VerticalScrollPercent => _pattern?.CurrentVerticalScrollPercent ?? 0;

		#endregion

		#region Methods

		/// <summary>
		/// Create a new pattern for the provided element.
		/// </summary>
		/// <param name="element"> The element that supports the pattern. </param>
		/// <returns> The pattern if we could find one else null will be returned. </returns>
		public static ScrollPattern Create(DesktopElement element)
		{
			var pattern = element.NativeElement.GetCurrentPattern(UIA_PatternIds.UIA_ScrollPatternId) as IUIAutomationScrollPattern;
			return pattern == null ? null : new ScrollPattern(pattern);
		}

		/// <summary>
		/// Scroll the desktop element.
		/// </summary>
		/// <param name="horizontalPercent"> The percentage to scroll horizontally. </param>
		/// <param name="verticalPercent"> The percentage to scroll vertically. </param>
		public void ScrollPercent(double horizontalPercent, double verticalPercent)
		{
			_pattern?.SetScrollPercent(horizontalPercent, verticalPercent);
		}

		#endregion
	}
}