#region References

using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation;
using Speedy.Automation.Desktop;

#endregion

namespace Speedy.AutomationTests.Desktop
{
	[TestClass]
	public class InputMouseTests
	{
		#region Methods

		[TestMethod]
		public void AbsoluteMoveMouse()
		{
			var primaryScreen = Screen.PrimaryScreen;

			var expected = new[]
			{
				new Point(0, 0), new Point(100, 101), new Point(500, 600),
				new Point(primaryScreen.Size.Width / 4, primaryScreen.Size.Height / 4),
				new Point(primaryScreen.Size.Width / 2, primaryScreen.Size.Height / 2),
				new Point(primaryScreen.Size.Width - 1, primaryScreen.Size.Height - 1)
			};

			foreach (var test in expected)
			{
				Input.Mouse.MoveTo(test);
				var actual = Input.Mouse.GetCursorPosition();
				Assert.AreEqual(test.X, actual.X);
				Assert.AreEqual(test.Y, actual.Y);
			}
		}

		[TestMethod]
		public void MoveToSecondaryMonitor()
		{
			var secondaryScreen = Screen.AllScreens.First(screen => !screen.IsPrimary);
			var x = secondaryScreen.Location.X + (secondaryScreen.Size.Width / 2);
			var y = secondaryScreen.Location.Y + (secondaryScreen.Size.Height / 2);

			Input.Mouse.MoveTo(x, y);

			var actual = Input.Mouse.GetCursorPosition();
			Assert.AreEqual(x, actual.X);
			Assert.AreEqual(y, actual.Y);

			//$"{x}, {y}".Dump();
		}

		#endregion
	}
}