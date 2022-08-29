#region References

using Interop.UIAutomationClient;

#endregion

namespace Speedy.Automation.Desktop.Pattern
{
	/// <summary>
	/// Represents the Windows value pattern.
	/// </summary>
	public class ValuePattern
	{
		#region Fields

		private readonly IUIAutomationValuePattern _pattern;

		#endregion

		#region Constructors

		private ValuePattern(IUIAutomationValuePattern pattern)
		{
			_pattern = pattern;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value determining if the pattern is read only.
		/// </summary>
		public bool IsReadOnly => _pattern.CurrentIsReadOnly == 1;

		/// <summary>
		/// Gets the value of the pattern.
		/// </summary>
		public string Value => _pattern.CurrentValue;

		#endregion

		#region Methods

		/// <summary>
		/// Create a new pattern for the provided element.
		/// </summary>
		/// <param name="element"> The element that supports the pattern. </param>
		/// <returns> The pattern if we could find one else null will be returned. </returns>
		public static ValuePattern Create(DesktopElement element)
		{
			var pattern = element.NativeElement.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId) as IUIAutomationValuePattern;
			return pattern == null ? null : new ValuePattern(pattern);
		}

		/// <summary>
		/// Set the value of the pattern.
		/// </summary>
		/// <param name="value"> The value to set. </param>
		public void SetValue(string value)
		{
			_pattern.SetValue(value);
		}

		#endregion
	}
}