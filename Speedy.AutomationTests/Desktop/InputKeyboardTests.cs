#region References

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation;
using Speedy.Automation.Desktop;
using Speedy.Automation.Tests;
using Speedy.UnitTests;

#endregion

namespace Speedy.AutomationTests.Desktop
{
	[TestClass]
	public class InputKeyboardTests
	{
		#region Methods

		[TestMethod]
		public void IsKeyDownAndIsKeyUp()
		{
			Assert.IsFalse(Input.Keyboard.IsKeyDown(KeyboardKey.A));
			Assert.IsTrue(Input.Keyboard.IsKeyUp(KeyboardKey.A));

			StartNotepad();

			Input.Keyboard.KeyDown(KeyboardKey.A);

			Assert.IsTrue(Input.Keyboard.IsKeyDown(KeyboardKey.A));
			Assert.IsFalse(Input.Keyboard.IsKeyUp(KeyboardKey.A));

			Input.Keyboard.KeyUp(KeyboardKey.A);

			Assert.IsFalse(Input.Keyboard.IsKeyDown(KeyboardKey.A));
			Assert.IsTrue(Input.Keyboard.IsKeyUp(KeyboardKey.A));

			CloseNotepad();
		}

		[TestMethod]
		public void NotepadInput()
		{
			var pressed = new StringBuilder();
			var actual = new StringBuilder();

			Input.Keyboard.KeyPressed += (sender, state) =>
			{
				pressed.AppendLine(state + ", pressed: " + state.IsPressed);

				if (state.IsPressed)
				{
					actual.Append(state);
				}
			};

			Input.Keyboard.StartMonitoring();

			try
			{
				using var application = Automation.Application.AttachOrCreate("c:\\windows\\system32\\notepad.exe");

				//var application = Automation.Application.AttachOrCreate("C:\\Workspaces\\GitHub\\Speedy\\Speedy.Winforms.Example\\bin\\Debug\\Speedy.Winforms.Example.exe");
				application.BringToFront();
				application.Focus();
				application.FirstOrDefault("textBox1", true, false)?.Focus();

				//var test = "\bf--   !@#$%^&*(){}";
				var test = "!\"#$%&\'()*+,-./:;<=>?@\\]^_`~		tabtab space";
				Input.Keyboard.SendInput(test);

				pressed.ToString().Dump();
				actual.ToString().Dump();
			}
			finally
			{
				Input.Keyboard.StopMonitoring();
				CloseNotepad();
			}
		}

		[TestMethod]
		public void SetNotepadTextAndMove()
		{
			StartNotepad();

			Input.Keyboard
				.SendInput("These are your orders if you choose to accept them...")
				.KeyPress(KeyboardKey.Enter)
				.SendInput("This message will self destruct in 1 seconds.")
				.Sleep(1000)
				.KeyPress(KeyboardModifier.Alt, KeyboardKey.Space)
				.KeyPress(KeyboardKey.DownArrow)
				.KeyPress(KeyboardKey.Return);

			for (var i = 0; i < 5; i++)
			{
				Input.Keyboard.KeyPress(KeyboardKey.DownArrow).Sleep(100);
			}

			for (var i = 0; i < 5; i++)
			{
				Input.Keyboard.KeyPress(KeyboardKey.UpArrow).Sleep(100);
			}

			Input.Keyboard.KeyPress(KeyboardKey.Return);

			CloseNotepad();
		}

		[TestMethod]
		public void SetNotepadTextAndMoveInOneCommand()
		{
			StartNotepad();

			const int delay = 100;

			Input.Keyboard
				.SendInput("These are your orders if you choose to accept them...", KeyboardKey.Enter)
				.SendInput("This message will self destruct in 1 seconds.",
					TimeSpan.FromMilliseconds(1000),
					new KeyStroke(KeyboardModifier.Alt, KeyboardKey.Space),
					new KeyStroke(KeyboardKey.DownArrow),
					new KeyStroke(KeyboardKey.Return)
				)
				.KeyPress(KeyboardKey.DownArrow).Sleep(delay)
				.KeyPress(KeyboardKey.DownArrow).Sleep(delay)
				.KeyPress(KeyboardKey.DownArrow).Sleep(delay)
				.KeyPress(KeyboardKey.DownArrow).Sleep(delay)
				.KeyPress(KeyboardKey.DownArrow).Sleep(delay)
				.KeyPress(KeyboardKey.UpArrow).Sleep(delay)
				.KeyPress(KeyboardKey.UpArrow).Sleep(delay)
				.KeyPress(KeyboardKey.UpArrow).Sleep(delay)
				.KeyPress(KeyboardKey.UpArrow).Sleep(delay)
				.KeyPress(KeyboardKey.UpArrow).Sleep(delay)
				.KeyPress(KeyboardKey.Enter);

			CloseNotepad();
		}

		private void CloseNotepad()
		{
			Input.Keyboard
				.KeyPress(KeyboardModifier.Alt, KeyboardKey.F4)
				.KeyPress(KeyboardKey.N);
		}

		private static void StartNotepad()
		{
			Input.Keyboard
				.KeyPress(KeyboardKey.LeftWindows, KeyboardKey.R)
				.Sleep(250)
				.SendInput("notepad")
				.Sleep(250)
				.KeyPress(KeyboardKey.Return)
				.Sleep(1000);
		}

		#endregion
	}
}