#region References

using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;

#endregion

namespace Speedy.UnitTests.Collections
{
	[TestClass]
	public class LimitedObservableCollectionTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void ThreadSafeCollectionAdd()
		{
			var count = 1000;
			var collection = new LimitedObservableCollection<int>(count);

			Parallel.For(0, count, (x, s) => { collection.Add(x); });

			var expected = Enumerable.Range(0, count).ToArray();
			TestHelper.AreEqual(expected, collection.OrderBy(x => x).ToArray());
		}

		#endregion
	}
}