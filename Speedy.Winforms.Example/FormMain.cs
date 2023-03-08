#region References

using System;
using System.Windows.Forms;
using Speedy.Automation;
using Speedy.Automation.Desktop;

#endregion

namespace Speedy.Winforms.Example
{
	public partial class FormMain : Form
	{
		#region Constructors

		public FormMain()
		{
			InitializeComponent();

			Input.Keyboard.KeyPressed += KeyboardOnKeyPressed;
			Input.Keyboard.StartMonitoring();
		}

		#endregion

		#region Methods

		private void ExitToolStripMenuItemClick(object sender, EventArgs e)
		{
			Close();
		}

		private void KeyboardOnKeyPressed(object sender, KeyboardState state)
		{
			if (!state.IsPressed)
			{
				return;
			}

			if (checkBoxKeyPressDetails.Checked)
			{
				keyPress.Text = state.ToDetailedString();
				keyPress.Text += Environment.NewLine;
				return;
			}

			keyPress.Text += state;
		}

		#endregion
	}
}