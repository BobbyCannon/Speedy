#region References

using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	public class BaseTests
	{
		#region Methods

		[TestInitialize]
		public virtual void TestInitialize()
		{
			TestHelper.Initialize();
		}

		protected void ClipboardSetText(string value)
		{
			var thread = new Thread(() =>
			{
				try
				{
					Clipboard.SetText(value);
				}
				catch
				{
					// Ignore the clipboard set issue...
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
		}

		protected string GetMessageAndCopy(string actual)
		{
			ClipboardSetText(actual);
			return actual;
		}

		#endregion
	}
}