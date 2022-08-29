#region References

using Interop.UIAutomationClient;
using Speedy.Automation.Desktop.Pattern;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents a scrollable desktop application.
	/// </summary>
	public abstract class ScrollableDesktopElement : DesktopElement, IScrollableElement
	{
		#region Fields

		private readonly ScrollPattern _scrollPattern;

		#endregion

		#region Constructors

		internal ScrollableDesktopElement(IUIAutomationElement element, Application application, DesktopElement parent)
			: base(element, application, parent)
		{
			_scrollPattern = ScrollPattern.Create(this);
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public double HorizontalScrollPercent => _scrollPattern?.HorizontalScrollPercent ?? 0;

		/// <inheritdoc />
		public bool IsScrollable => _scrollPattern != null;

		/// <inheritdoc />
		public double VerticalScrollPercent => _scrollPattern?.VerticalScrollPercent ?? 0;

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Scroll(double horizontalPercent, double verticalPercent)
		{
			_scrollPattern?.ScrollPercent(horizontalPercent, verticalPercent);
		}

		#endregion
	}
}