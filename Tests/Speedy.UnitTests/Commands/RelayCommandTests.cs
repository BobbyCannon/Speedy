using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Commands;

namespace Speedy.UnitTests.Commands;

[TestClass]
public class RelayCommandTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void CanExecute()
	{
		var command = new RelayCommand(_ => { }, x => true);
		IsTrue(command.CanExecute(new object()));
	}

	#endregion
}