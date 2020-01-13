#region References

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

		#endregion
	}
}