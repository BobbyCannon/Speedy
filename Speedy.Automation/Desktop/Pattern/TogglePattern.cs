#region References

using Interop.UIAutomationClient;
using Speedy.Automation.Internal;

#endregion

namespace Speedy.Automation.Desktop.Pattern
{
	/// <summary>
	/// Represents the Windows toggle pattern.
	/// </summary>
	public class TogglePattern
	{
		#region Fields

		private readonly IUIAutomationTogglePattern _pattern;

		#endregion

		#region Constructors

		private TogglePattern(IUIAutomationTogglePattern pattern)
		{
			_pattern = pattern;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the toggled value.
		/// </summary>
		public bool Toggled => _pattern.CurrentToggleState.Convert() != ToggleState.Off;

		/// <summary>
		/// Gets the toggle state of the element.
		/// </summary>
		public ToggleState ToggleState => _pattern.CurrentToggleState.Convert();

		#endregion

		#region Methods

		/// <summary>
		/// Create a new pattern for the provided element.
		/// </summary>
		/// <param name="element"> The element that supports the pattern. </param>
		/// <returns> The pattern if we could find one else null will be returned. </returns>
		public static TogglePattern Create(DesktopElement element)
		{
			var pattern = element.NativeElement.GetCurrentPattern(UIA_PatternIds.UIA_TogglePatternId) as IUIAutomationTogglePattern;
			return pattern == null ? null : new TogglePattern(pattern);
		}

		/// <summary>
		/// Toggle the element.
		/// </summary>
		public void Toggle()
		{
			_pattern.Toggle();
		}

		#endregion
	}
}