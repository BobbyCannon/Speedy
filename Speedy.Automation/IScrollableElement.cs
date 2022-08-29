namespace Speedy.Automation
{
	/// <summary>
	/// Represents a scrollable element.
	/// </summary>
	public interface IScrollableElement
	{
		#region Properties

		/// <summary>
		/// Gets the scroll percentage for the horizontal bar.
		/// </summary>
		double HorizontalScrollPercent { get; }

		/// <summary>
		/// Gets a flag indicating the element is scrollable.
		/// </summary>
		bool IsScrollable { get; }

		/// <summary>
		/// Gets the scroll percentage for the vertical bar.
		/// </summary>
		double VerticalScrollPercent { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Scroll the browser window.
		/// </summary>
		/// <param name="horizontalPercent"> The percentage to scroll horizontally. </param>
		/// <param name="verticalPercent"> The percentage to scroll vertically. </param>
		void Scroll(double horizontalPercent, double verticalPercent);

		#endregion
	}
}