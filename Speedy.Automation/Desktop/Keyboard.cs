#region References

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Speedy.Automation.Internal.Native;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents the keyboard and allows for simulated input.
	/// </summary>
	public class Keyboard : IDisposable
	{
		#region Fields

		private NativeInput.KeyboardHookDelegate _keyboardCallback;
		private IntPtr _keyboardHandle;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Keyboard" />.
		/// </summary>
		public Keyboard()
		{
			State = new KeyboardState();
			DefaultInputDelay = TimeSpan.Zero;
			TextInputAsKeyPresses = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Default input delay if the SendInput delay is TimeSpan.Zero.
		/// If this default input delay is also TimeSpan.Zero then no delay will occur.
		/// </summary>
		public TimeSpan DefaultInputDelay { get; set; }

		/// <summary>
		/// The last state of the keyboard when monitoring.
		/// </summary>
		public KeyboardState State { get; }

		/// <summary>
		/// Defaults the keyboard SendInput to send text as key presses if true.
		/// Otherwise the send input will send the text as "
		/// </summary>
		public bool TextInputAsKeyPresses { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Returns a string with all printable characters.
		/// </summary>
		/// <returns> All printable characters in order by KeyboardKey. </returns>
		public static string GetAllPrintableCharacters()
		{
			var printableCharacters = new StringBuilder();

			foreach (var key in Enum.GetValues(typeof(KeyboardKey)).Cast<KeyboardKey>())
			{
				var state = new KeyboardState { Key = key };
				var value = state.ToString();
				if (value != null)
				{
					printableCharacters.Append(value);
				}

				state.Character = null;
				state.IsLeftShiftPressed = true;
				value = state.ToString();

				if (value != null)
				{
					printableCharacters.Append(value);
				}
			}

			return printableCharacters.ToString();
		}

		/// <summary>
		/// Determines whether the specified key is up or down.
		/// </summary>
		/// <param name="keys"> The <see cref="KeyboardKey" /> for the key(s). </param>
		/// <returns>
		/// True if the key is down otherwise false.
		/// </returns>
		public bool IsKeyDown(params KeyboardKey[] keys)
		{
			return keys.Aggregate(false, (current, key) => current | (NativeInput.GetKeyState((ushort) key) < 0));
		}

		/// <summary>
		/// Determines whether the specified key is up or down.
		/// </summary>
		/// <param name="keys"> The <see cref="KeyboardKey" /> for the key(s). </param>
		/// <returns>
		/// True if the key is up otherwise false.
		/// </returns>
		public bool IsKeyUp(params KeyboardKey[] keys)
		{
			return !IsKeyDown(keys);
		}

		/// <summary>
		/// Determines whether the toggling key is toggled on (in-effect) or not by calling the GetKeyState function.
		/// </summary>
		/// <param name="key"> The <see cref="KeyboardKey" /> for the key. </param>
		/// <returns>
		/// True if the toggling key is toggled on (in-effect) otherwise false.
		/// </returns>
		public bool IsTogglingKeyInEffect(KeyboardKey key)
		{
			var result = NativeInput.GetKeyState((ushort) key);
			return (result & 0x01) == 0x01;
		}

		/// <summary>
		/// Calls the Input.SendInput method to simulate key down.
		/// </summary>
		/// <param name="keys"> The key(s) to press down. </param>
		public Keyboard KeyDown(params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder().AddKeyDown(keys));

			return this;
		}

		/// <summary>
		/// Simulates a key press for each of the specified key codes in the order they are specified.
		/// </summary>
		/// <param name="keys"> The keys to press. </param>
		public Keyboard KeyPress(params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder(keys));

			return this;
		}

		/// <summary>
		/// Simulates a modified keystroke where there is one modifier and multiple keys like CTRL-K-C where CTRL is the
		/// modifierKey and K and C are the keys.
		/// The flow is Modifier KeyDown, Keys Press in order, Modifier KeyUp.
		/// </summary>
		/// <param name="modifier"> The modifier key. </param>
		/// <param name="keys"> The list of keys to press. </param>
		public Keyboard KeyPress(KeyboardModifier modifier, params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder(modifier, keys));

			return this;
		}

		/// <summary>
		/// Calls the Input.SendInput method to simulate key up.
		/// </summary>
		/// <param name="keys"> The key(s) to lift up. </param>
		public Keyboard KeyUp(params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder().AddKeyUp(keys));

			return this;
		}

		/// <summary>
		/// Sends provided text and an optional set of keys as input.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard SendInput(string text)
		{
			Input.SendInput(new InputBuilder(text, TextInputAsKeyPresses), TimeSpan.Zero);
			return this;
		}

		/// <summary>
		/// Sends provided text and an optional set of keys as input.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <param name="delay"> An optional delay after sending input. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard SendInput(string text, TimeSpan delay)
		{
			Input.SendInput(new InputBuilder(text, TextInputAsKeyPresses), delay);
			return this;
		}

		/// <summary>
		/// Sends provided text and an optional set of keys as input.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <param name="keys"> The set of keys to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard SendInput(string text, params KeyboardKey[] keys)
		{
			return SendInput(text, TimeSpan.Zero, keys);
		}

		/// <summary>
		/// Sends provided text as input. Can delay with before sending an optional set of keys.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <param name="delay"> An optional delay to wait before sending the provided keys. </param>
		/// <param name="keys"> An optional set of keys to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		/// <exception cref="ArgumentException"> The text parameter is too long. </exception>
		public Keyboard SendInput(string text, TimeSpan delay, params KeyboardKey[] keys)
		{
			if (text.Length > (uint.MaxValue / 2))
			{
				throw new ArgumentException($"The text parameter is too long. It must be less than {uint.MaxValue / 2} characters.", nameof(text));
			}

			var inputList = Input.SendInput(new InputBuilder(text, TextInputAsKeyPresses), delay);

			if ((keys != null) && (keys.Length > 0))
			{
				Input.SendInput(inputList.Clear().AddKeyPress(keys));
			}

			return this;
		}

		/// <summary>
		/// Sends provided text and optional set of key strokes as input.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <param name="keyStrokes"> An optional set of key strokes to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		/// <exception cref="ArgumentException"> The text parameter is too long. </exception>
		public Keyboard SendInput(string text, params KeyStroke[] keyStrokes)
		{
			return SendInput(text, TimeSpan.Zero, keyStrokes);
		}

		/// <summary>
		/// Sends provided text as input. Can delay with before sending an optional set of key strokes.
		/// </summary>
		/// <param name="text"> The text to be sent. </param>
		/// <param name="delay"> An optional delay to wait before sending the provided keys. </param>
		/// <param name="keyStrokes"> An optional set of key strokes to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		/// <exception cref="ArgumentException"> The text parameter is too long. </exception>
		public Keyboard SendInput(string text, TimeSpan delay, params KeyStroke[] keyStrokes)
		{
			if (text.Length > (uint.MaxValue / 2))
			{
				throw new ArgumentException($"The text parameter is too long. It must be less than {uint.MaxValue / 2} characters.", nameof(text));
			}

			var inputList = Input.SendInput(new InputBuilder(text, TextInputAsKeyPresses), delay);

			if (keyStrokes.Length > 0)
			{
				Input.SendInput(inputList.Clear().Add(keyStrokes));
			}

			return this;
		}

		/// <summary>
		/// Sends provided set of key as input.
		/// </summary>
		/// <param name="keys"> The set of keys to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard SendInput(params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder(keys));

			return this;
		}

		/// <summary>
		/// Sends provided set of keys as input with a modifier (ctrl, shift, etc).
		/// </summary>
		/// <param name="modifiers"> The modifier key(s). </param>
		/// <param name="keys"> The set of keys to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard SendInput(KeyboardModifier modifiers, params KeyboardKey[] keys)
		{
			if (keys.Length <= 0)
			{
				return this;
			}

			Input.SendInput(new InputBuilder(modifiers, keys));

			return this;
		}

		/// <summary>
		/// Sends provided key strokes as input.
		/// </summary>
		/// <param name="keyStrokes"> The set of key strokes to be sent. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		/// <exception cref="ArgumentException"> The text parameter is too long. </exception>
		public Keyboard SendInput(params KeyStroke[] keyStrokes)
		{
			if (keyStrokes.Length > 0)
			{
				Input.SendInput(new InputBuilder(keyStrokes));
			}

			return this;
		}

		/// <summary>
		/// Sleeps the executing thread to create a pause between simulated inputs.
		/// </summary>
		/// <param name="timeoutInMilliseconds"> The number of milliseconds to wait. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard Sleep(int timeoutInMilliseconds)
		{
			Thread.Sleep(timeoutInMilliseconds);
			return this;
		}

		/// <summary>
		/// Sleeps the executing thread to create a pause between simulated inputs.
		/// </summary>
		/// <param name="timeout"> The time to wait. </param>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard Sleep(TimeSpan timeout)
		{
			Thread.Sleep(timeout);
			return this;
		}

		/// <summary>
		/// Start monitoring the keyboard input.
		/// </summary>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard StartMonitoring()
		{
			const int lowLevelKeyboardHook = 13;

			_keyboardCallback = KeyboardHookCallback;
			_keyboardHandle = NativeInput.SetWindowsHookEx(lowLevelKeyboardHook, _keyboardCallback, IntPtr.Zero, 0);

			State.IsMonitoring = true;

			return this;
		}

		/// <summary>
		/// Stop monitoring the keyboard input.
		/// </summary>
		/// <returns> This <see cref="Keyboard" /> instance. </returns>
		public Keyboard StopMonitoring()
		{
			if (_keyboardHandle != IntPtr.Zero)
			{
				NativeInput.UnhookWindowsHookEx(_keyboardHandle);
				_keyboardHandle = IntPtr.Zero;
			}

			State.IsMonitoring = false;

			return this;
		}

		/// <summary>
		/// Convert a keyboard key to a character
		/// </summary>
		/// <param name="key"> The keyboard key </param>
		/// <param name="state"> </param>
		/// <returns> </returns>
		public static char? ToCharacter(KeyboardKey key, KeyboardState state)
		{
			var altKey = state.IsCapsLockOn || state.IsShiftPressed;

			if ((key >= KeyboardKey.A) && (key <= KeyboardKey.Z))
			{
				return altKey ? (char) key : ((char) key).ToString().ToLower()[0];
			}

			if ((key >= KeyboardKey.Number0) && (key <= KeyboardKey.Number9))
			{
				if (state.IsShiftPressed)
				{
					return key switch
					{
						KeyboardKey.Number1 => '!',
						KeyboardKey.Number2 => '@',
						KeyboardKey.Number3 => '#',
						KeyboardKey.Number4 => '$',
						KeyboardKey.Number5 => '%',
						KeyboardKey.Number6 => '^',
						KeyboardKey.Number7 => '&',
						KeyboardKey.Number8 => '*',
						KeyboardKey.Number9 => '(',
						KeyboardKey.Number0 => ')',
						_ => (char) key
					};
				}

				return (char) key;
			}

			if ((key >= KeyboardKey.Numpad0) && (key <= KeyboardKey.Numpad9))
			{
				return (char) (key - 0x30);
			}

			return key switch
			{
				KeyboardKey.Add => '+',
				KeyboardKey.Decimal => '.',
				KeyboardKey.Divide => '/',
				KeyboardKey.Subtract => '-',
				KeyboardKey.Space => ' ',
				KeyboardKey.Tab => '\t',
				KeyboardKey.Oem1 => state.IsShiftPressed ? ':' : ';',
				KeyboardKey.Oem2 => state.IsShiftPressed ? '?' : '/',
				KeyboardKey.Oem3 => state.IsShiftPressed ? '~' : '`',
				KeyboardKey.Oem4 => state.IsShiftPressed ? '{' : '[',
				KeyboardKey.Oem5 => state.IsShiftPressed ? '|' : '\\',
				KeyboardKey.Oem6 => state.IsShiftPressed ? '}' : ']',
				KeyboardKey.Oem7 => state.IsShiftPressed ? '"' : '\'',
				KeyboardKey.OemComma => state.IsShiftPressed ? '<' : ',',
				KeyboardKey.OemMinus => state.IsShiftPressed ? '_' : '-',
				KeyboardKey.OemPeriod => state.IsShiftPressed ? '>' : '.',
				KeyboardKey.OemPlus => state.IsShiftPressed ? '+' : '=',
				_ => null
			};
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			StopMonitoring();
		}

		private int KeyboardHookCallback(int code, int wParam, ref NativeInput.KeyboardHookStruct lParam)
		{
			const int wParamKeydown = 0x0100;

			if (code >= 0)
			{
				Debug.WriteLine($"Code: {code}, wParam: {wParam}, lParam.vkCode: {lParam.vkCode}; flags: {lParam.flags}");

				var key = (KeyboardKey) lParam.vkCode;
				var isPressed = wParam == wParamKeydown;

				switch (key)
				{
					case KeyboardKey.CapsLock:
						State.IsCapsLockOn = IsTogglingKeyInEffect(KeyboardKey.CapsLock);
						break;

					case KeyboardKey.Alt:
						State.IsLeftAltPressed = isPressed;
						State.IsRightAltPressed = isPressed;
						break;

					case KeyboardKey.LeftAlt:
						State.IsLeftAltPressed = isPressed;
						break;

					case KeyboardKey.RightAlt:
						State.IsRightAltPressed = isPressed;
						break;

					case KeyboardKey.Control:
						State.IsLeftControlPressed = isPressed;
						State.IsRightControlPressed = isPressed;
						break;

					case KeyboardKey.LeftControl:
						State.IsLeftControlPressed = isPressed;
						break;

					case KeyboardKey.RightControl:
						State.IsRightControlPressed = isPressed;
						break;

					case KeyboardKey.Shift:
						State.IsLeftShiftPressed = isPressed;
						State.IsRightShiftPressed = isPressed;
						break;

					case KeyboardKey.LeftShift:
						State.IsLeftShiftPressed = isPressed;
						break;

					case KeyboardKey.RightShift:
						State.IsRightShiftPressed = isPressed;
						break;
				}

				var character = ToCharacter(key, State);

				if (key == KeyboardKey.Packet)
				{
					var scanCode = (char) lParam.scanCode;
					key = (KeyboardKey) (NativeInput.VkKeyScan(scanCode) & 0xFF);
				}

				State.Character = character;
				State.Key = key;
				State.IsPressed = isPressed;

				// Debug.WriteLine(State.ToDebugString());

				KeyPressed?.Invoke(this, State.Clone());
			}

			return NativeInput.CallNextHookEx(_keyboardHandle, code, wParam, ref lParam);
		}

		#endregion

		#region Events

		/// <summary>
		/// Called when monitoring keyboard and a key is pressed.
		/// </summary>
		public event EventHandler<KeyboardState> KeyPressed;

		#endregion
	}
}