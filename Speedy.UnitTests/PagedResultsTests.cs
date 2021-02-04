#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PagedResultsTests
	{
		#region Methods

		[TestMethod]
		public void CalculatePaginationValues()
		{
			// Should only have 1 page
			var results = new PagedResults { Page = 1, PerPage = 10, TotalCount = 10 };
			var actual = results.CalculatePaginationValues();
			Assert.AreEqual(1, actual.start);
			Assert.AreEqual(1, actual.end);

			// Should only show first 5 pages
			results = new PagedResults { Page = 1, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(1, actual.start);
			Assert.AreEqual(5, actual.end);

			// Should only show last 5 pages
			results = new PagedResults { Page = 10, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(6, actual.start);
			Assert.AreEqual(10, actual.end);

			// Should only almost first 5 pages
			results = new PagedResults { Page = 4, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(2, actual.start);
			Assert.AreEqual(6, actual.end);

			// Should only almost last 5 pages
			results = new PagedResults { Page = 7, PerPage = 10, TotalCount = 100 };
			actual = results.CalculatePaginationValues();
			Assert.AreEqual(5, actual.start);
			Assert.AreEqual(9, actual.end);
		}

		[TestMethod]
		public void HasMore()
		{
			var actual = new PagedResults { Page = 1, PerPage = 10, TotalCount = 10 };
			Assert.AreEqual(false, actual.HasMore);

			actual = new PagedResults { Page = 1, PerPage = 10, TotalCount = 11 };
			Assert.AreEqual(true, actual.HasMore);

			actual = new PagedResults { Page = 10, PerPage = 1, TotalCount = 11 };
			Assert.AreEqual(true, actual.HasMore);

			// This is not a valid result but should still "not" have more data
			actual = new PagedResults { Page = 12, PerPage = 1, TotalCount = 11 };
			Assert.AreEqual(false, actual.HasMore);
		}

		[TestMethod]
		public void TotalPages()
		{
			var actual = new PagedResults { Page = 1, PerPage = 10, TotalCount = 10 };
			Assert.AreEqual(1, actual.TotalPages);

			actual = new PagedResults { Page = 1, PerPage = 10, TotalCount = 11 };
			Assert.AreEqual(2, actual.TotalPages);

			actual = new PagedResults { Page = 10, PerPage = 1, TotalCount = 10 };
			Assert.AreEqual(10, actual.TotalPages);

			actual = new PagedResults { Page = 12, PerPage = 1, TotalCount = 10 };
			Assert.AreEqual(10, actual.TotalPages);
		}

		#endregion
	}
}