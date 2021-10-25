#region References

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.WebApi;
using Speedy.UnitTests.Factories;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedRequestTests
	{
		#region Methods

		[TestMethod]
		public void Cleanup()
		{
			var actual = new PagedRequest
			{
				Page = 0,
				PerPage = 0,
				Filter = null,
				FilterValues = null,
				Including = null,
				Options = null,
				OptionValues = null,
				Order = null
			};

			Assert.AreEqual(0, actual.Page);
			Assert.AreEqual(0, actual.PerPage);
			Assert.AreEqual(null, actual.Filter);
			Assert.AreEqual(null, actual.FilterValues);
			Assert.AreEqual(null, actual.Including);
			Assert.AreEqual(null, actual.Options);
			Assert.AreEqual(null, actual.OptionValues);
			Assert.AreEqual(null, actual.Order);

			actual.Cleanup();

			var expected = new PagedRequest
			{
				Page = 1,
				PerPage = 10,
				Filter = string.Empty,
				FilterValues = new List<string>(),
				Including = new List<string>(),
				Options = new List<string>(),
				OptionValues = new List<string>(),
				Order = string.Empty
			};

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TypedData()
		{
			var actual = new PagedResults<Address>
			{
				Results = new[] { DataFactory.GetAddress() }
			};
			Assert.AreEqual(1, actual.Results.Count());
		}

		#endregion
	}
}