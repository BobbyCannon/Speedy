namespace Speedy.Automation.Desktop.Pattern
{
	/// <summary>
	/// Represents the state of a element with toggle pattern support.
	/// </summary>
	public enum ToggleState
	{
		/// <summary>
		/// The element is selected, checked, marked or otherwise activated.
		/// </summary>
		On,

		/// <summary>
		/// The element is not selected, checked, marked or otherwise activated.
		/// </summary>
		Off,

		/// <summary>
		/// The element is in an indeterminate state.
		/// </summary>
		Indeterminate
	}
}