#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;

#endregion

namespace Speedy.UnitTests;

public class SpeedyUnitTest : SpeedyTest
{
	#region Properties

	public static string NotepadPath => @"C:\Program Files\WindowsApps\Microsoft.WindowsNotepad_11.2302.16.0_x64__8wekyb3d8bbwe\Notepad\Notepad.exe";

	#endregion

	#region Methods

	[TestInitialize]
	public virtual void TestInitialize()
	{
		TestHelper.Initialize();
	}

	#endregion
}

public class SpeedyUnitTest<T> : SpeedyTest<T> where T : new()
{
	#region Methods

	[TestInitialize]
	public virtual void TestInitialize()
	{
		TestHelper.Initialize();
	}

	#endregion
}