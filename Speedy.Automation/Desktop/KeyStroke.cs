namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents a key stroke.
	/// </summary>
	public class KeyStroke
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of a key stroke.
		/// </summary>
		/// <param name="key"> The key to be processed. </param>
		public KeyStroke(KeyboardKey key) : this(KeyboardModifier.None, key)
		{
		}

		/// <summary>
		/// Instantiates an instance of a key stroke.
		/// </summary>
		/// <param name="modifier"> An optional modifier. </param>
		/// <param name="key"> The key to be processed. </param>
		public KeyStroke(KeyboardModifier modifier, KeyboardKey key)
		{
			Action = KeyboardAction.KeyPress;
			Modifier = modifier;
			Key = key;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The keyboard action to perform. Defaults to KeyPress.
		/// </summary>
		public KeyboardAction Action { get; set; }

		/// <summary>
		/// The key to be processed.
		/// </summary>
		public KeyboardKey Key { get; set; }

		/// <summary>
		/// An optional key modifier. Ex. Control, Alt, Shift
		/// </summary>
		public KeyboardModifier Modifier { get; set; }

		#endregion
	}
}