#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.ServiceHosting.Example;

#endregion

namespace Speedy.UnitTests.ServiceHosting.Example
{
	[TestClass]
	public class ServiceOptionsTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void StartOption()
		{
			var options = GetOptions("-start", "12");
			Assert.AreEqual(12, options.Start);
			
			options = GetOptions("-start");
			Assert.AreEqual(0, options.Start);
		}

		private ServiceOptions GetOptions(params string[] arguments)
		{
			var options = new ServiceOptions();
			options.Initialize(arguments);
			return options;
		}

		#endregion
	}
}