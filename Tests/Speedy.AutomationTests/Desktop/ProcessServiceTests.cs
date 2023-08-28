#region References

using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Desktop;
using Speedy.Automation.Tests;
using Speedy.UnitTests;

#endregion

namespace Speedy.AutomationTests.Desktop
{
	[TestClass]
	public class ProcessServiceTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ListAllProcesses()
		{
			var processes = ProcessService.GetAllProcesses();
			foreach (var process in processes)
			{
				var test = process.FileName + ": " + process.FilePath;
				test.Dump();
				process.Dispose();
			}
		}

		[TestMethod]
		public void WhereShouldFindByName()
		{
			Automation.Application.CloseAll(NotepadPath);

			var processes = ProcessService.Where("Notepad.exe").ToList();
			Assert.AreEqual(0, processes.Count);

			using (var a = Automation.Application.Create(NotepadPath))
			{
				var watch = Stopwatch.StartNew();
				processes = ProcessService.Where("Notepad.exe").ToList();
				watch.Stop();
				watch.Elapsed.Dump();
				a.Close();

				Assert.AreEqual(1, processes.Count);
			}
		}

		#endregion
	}
}