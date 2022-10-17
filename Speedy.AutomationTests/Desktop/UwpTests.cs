#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Desktop;
using Speedy.Automation.Desktop.Elements;
using Speedy.UnitTests;

#endregion

namespace Speedy.AutomationTests.Desktop
{
	[TestClass]
	public class UwpTests
	{
		#region Methods

		[TestMethod]
		public void LaunchUwpApp()
		{
			var filepath = TestHelper.ApplicationPathForUwp;
			var packageName = "5495f851-725d-41f7-b34a-889a800e2315_1myjjx7jt00bj";
			var expected = "C:\\Workspaces\\GitHub\\Speedy\\Speedy.TestUwp\\bin\\x86\\Debug\\AppX\\Speedy.TestUwp.exe";

			Assert.AreEqual(expected, filepath);

			var process = Automation.Application.Attach(filepath, isUwp: true);
			if (process != null)
			{
				process.Kill(1);
				process.Dispose();
			}

			var application = Automation.Application.AttachOrCreateUniversal(filepath, packageName);

			application.Children.Count.Dump();
			var children = application.Descendants().ToList();
			children.Count.Dump();
			children.ForEach(x =>
			{
				x.FullId.Dump();
				x.GetType().Dump();
			});

			application.FirstOrDefault<Edit>("FirstInput").SetText(DateTime.UtcNow.ToString());
			application.BringToFront();
			application.Dispose();
		}

		[TestMethod]
		public void ListProcesses()
		{
			var processes = ProcessService.WhereUniversal("C:\\Workspaces\\GitHub\\Speedy\\Speedy.TestUwp\\bin\\x86\\Debug\\AppX\\Speedy.TestUwp.exe");
			foreach (var p in processes)
			{
				$"{p.FileName}: {p.FilePath}".Dump();
			}
		}

		#endregion
	}
}