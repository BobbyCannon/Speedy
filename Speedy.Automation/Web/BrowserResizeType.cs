namespace Speedy.Automation.Web
{
	/// <summary>
	/// Represents browser resize types
	/// </summary>
	public enum BrowserResizeType
	{
		/// <summary>
		/// Places browser side by side horizontally across the whole screen.
		/// </summary>
		SideBySide = 0,

		/// <summary>
		/// Places browser side by side horizontally across the left side screen.
		/// If more than two browsers then it will create quarters.
		/// </summary>
		LeftSideBySide = 1,

		/// <summary>
		/// Places browser side by side horizontally across the right side screen.
		/// If more than two browsers then it will create quarters.
		/// </summary>
		RightSideBySide = 2
	}
}