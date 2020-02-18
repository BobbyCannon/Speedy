#region References

using System.Windows.Forms;
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

		protected string GetMessageAndCopy(string actual)
		{
			Clipboard.SetText(actual);
			return actual;
		}

		#endregion
	}
}