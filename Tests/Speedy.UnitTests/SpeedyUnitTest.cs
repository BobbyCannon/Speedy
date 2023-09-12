#region References

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests;

public class SpeedyUnitTest : SpeedyTest
{
	#region Constructors

	static SpeedyUnitTest()
	{
		var assembly = typeof(SpeedyUnitTest).Assembly;

		AssemblyDirectory = assembly.GetAssemblyDirectory();
		UnitTestsDirectory = AssemblyDirectory.Parent?.Parent?.Parent?.FullName;
	}

	#endregion

	#region Properties

	public static DirectoryInfo AssemblyDirectory { get; }

	public static string NotepadPath => @"C:\Program Files\WindowsApps\Microsoft.WindowsNotepad_11.2302.16.0_x64__8wekyb3d8bbwe\Notepad\Notepad.exe";

	public static string UnitTestsDirectory { get; }

	#endregion

	#region Methods

	[TestInitialize]
	public override void TestInitialize()
	{
		TestHelper.Initialize();

		base.TestInitialize();
	}

	#endregion
}

public class SpeedyUnitTest<T> : SpeedyUnitTest where T : new()
{
	#region Methods

	/// <summary>
	/// Get a model of the provided type.
	/// </summary>
	/// <returns> An instance of the type. </returns>
	protected T GetModel()
	{
		var response = new T();
		return response;
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <returns> The instance of the type with non default values. </returns>
	protected T GetModelWithNonDefaultValues()
	{
		return GetModelWithNonDefaultValues<T>();
	}

	#endregion
}