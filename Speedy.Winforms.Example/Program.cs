#region References

using System;
using FormApplication = System.Windows.Forms.Application;

#endregion

namespace Speedy.Winforms.Example
{
	internal static class Program
	{
		#region Methods

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			FormApplication.EnableVisualStyles();
			FormApplication.SetCompatibleTextRenderingDefault(false);
			FormApplication.Run(new ParentForm());
		}

		#endregion
	}
}