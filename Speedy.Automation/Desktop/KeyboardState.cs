#region References

using System;
using Speedy.Automation.Internal.Native;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents the state of the keyboard during Keyboard.StartMonitoring.
	/// </summary>
	public class KeyboardState : IComparable<KeyboardState>, IComparable, IEquatable<KeyboardState>
	{
		#region Properties

		/// <summary>
		/// The string interpretation of the key.
		/// </summary>
		public char? Character { get; set; }

		/// <summary>
		/// Gets a value indicating if either the left or right alt key is pressed.
		/// </summary>
		public bool IsAltPressed => IsLeftAltPressed || IsRightAltPressed;

		/// <summary>
		/// Determines if the caps lock in on at the time of the key event.
		/// </summary>
		public bool IsCapsLockOn { get; set; }

		/// <summary>
		/// Gets a value indicating if either the left or right control key is pressed.
		/// </summary>
		public bool IsControlPressed => IsLeftControlPressed || IsRightControlPressed;

		/// <summary>
		/// Gets a value indicating if the left alt key is pressed.
		/// </summary>
		public bool IsLeftAltPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the left control key is pressed.
		/// </summary>
		public bool IsLeftControlPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the left shift key is pressed.
		/// </summary>
		public bool IsLeftShiftPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the keyboard input is being monitored.
		/// </summary>
		public bool IsMonitoring { get; set; }

		/// <summary>
		/// Gets a value indicating the key is being pressed (down). If false the key is being released (up).
		/// </summary>
		public bool IsPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the right alt key is pressed.
		/// </summary>
		public bool IsRightAltPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the right control key is pressed.
		/// </summary>
		public bool IsRightControlPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if the right shift key is pressed.
		/// </summary>
		public bool IsRightShiftPressed { get; set; }

		/// <summary>
		/// Gets a value indicating if either the left or right shift key is pressed.
		/// </summary>
		public bool IsShiftPressed => IsLeftShiftPressed || IsRightShiftPressed;

		/// <summary>
		/// Gets a value of the key being changed (up or down).
		/// </summary>
		public KeyboardKey Key { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Clone the keyboard state.
		/// </summary>
		/// <returns> The copy of the keyboard state. </returns>
		public KeyboardState Clone()
		{
			return new KeyboardState
			{
				Character = Character,
				IsCapsLockOn = IsCapsLockOn,
				IsLeftAltPressed = IsLeftAltPressed,
				IsLeftControlPressed = IsLeftControlPressed,
				IsLeftShiftPressed = IsLeftShiftPressed,
				IsMonitoring = IsMonitoring,
				IsPressed = IsPressed,
				IsRightAltPressed = IsRightAltPressed,
				IsRightControlPressed = IsRightControlPressed,
				IsRightShiftPressed = IsRightShiftPressed,
				Key = Key
			};
		}

		/// <inheritdoc />
		public int CompareTo(KeyboardState other)
		{
			return Equals(other) ? 0 : 1;
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			return Equals(obj) ? 0 : 1;
		}

		/// <inheritdoc />
		public bool Equals(KeyboardState state)
		{
			return (Character == state.Character) &&
				(IsAltPressed == state.IsAltPressed) &&
				(IsCapsLockOn == state.IsCapsLockOn) &&
				(IsControlPressed == state.IsControlPressed) &&
				(IsLeftAltPressed == state.IsLeftAltPressed) &&
				(IsLeftControlPressed == state.IsLeftControlPressed) &&
				(IsLeftShiftPressed == state.IsLeftShiftPressed) &&
				(IsMonitoring == state.IsMonitoring) &&
				(IsPressed == state.IsPressed) &&
				(IsRightAltPressed == state.IsRightAltPressed) &&
				(IsRightControlPressed == state.IsRightControlPressed) &&
				(IsRightShiftPressed == state.IsRightShiftPressed) &&
				(IsShiftPressed == state.IsShiftPressed) &&
				(Key == state.Key);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return Equals(obj as KeyboardState);
		}

		/// <summary>
		/// Instantiates a keyboard state from a character value.
		/// </summary>
		public static KeyboardState FromCharacter(char? character)
		{
			var response = new KeyboardState();
			var vk = character != null ? NativeInput.VkKeyScan(character.Value) : 0;

			response.Character = character;
			response.IsLeftShiftPressed = (vk & 0x0100) == 0x0100;
			response.Key = (KeyboardKey) (vk & 0xFF);

			return response;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			var hashCode = 777250742;
			hashCode = (hashCode * -1521134295) + Character.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsAltPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsCapsLockOn.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsControlPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsLeftAltPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsLeftControlPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsLeftShiftPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsMonitoring.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsRightAltPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsRightControlPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsRightShiftPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsShiftPressed.GetHashCode();
			hashCode = (hashCode * -1521134295) + Key.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// Gets the keyboard modifier for this state.
		/// </summary>
		/// <returns> The keyboard modifier. </returns>
		public KeyboardModifier GetKeyboardModifier()
		{
			var response = KeyboardModifier.None;

			if (IsLeftShiftPressed)
			{
				response |= KeyboardModifier.LeftShift;
			}

			if (IsRightShiftPressed)
			{
				response |= KeyboardModifier.RightShift;
			}

			return response;
		}

		/// <summary>
		/// To a details string for this keyboard state.
		/// </summary>
		/// <returns> </returns>
		public string ToDetailedString()
		{
			return $"Key: KeyboardState.{Key}, Character: {Character}";
		}

		/// <inheritdoc />
		public override string ToString()
		{
			Character ??= Keyboard.ToCharacter(Key, this);
			return Character?.ToString();
		}

		#endregion
	}
}