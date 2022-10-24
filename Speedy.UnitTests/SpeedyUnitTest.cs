#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;

#endregion

namespace Speedy.UnitTests
{
	public class SpeedyUnitTest : SpeedyTest
	{
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
}