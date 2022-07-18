#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Serialization;
using Speedy.Serialization.Converters;
using Speedy.Website.Models;
using System.Linq;

#endregion

namespace Speedy.UnitTests.Serialization.Converters
{
	[TestClass]
	public class PartialUpdateConverterTests
	{
		#region Methods

		[TestMethod]
		public void CamelcaseShouldWork()
		{
			var settings = SerializerSettings.DefaultSettings;

			Assert.AreEqual(1, settings.JsonSettings.Converters.Count(x => x is PartialUpdateConverter));

			var update = new CustomPagedRequest
			{
				Page = 3,
				PerPage = 12,
				Precision = 321.45
			};

			var expected = "{\"$id\":\"1\",\"filter\":\"\",\"order\":\"\",\"page\":3,\"perPage\":12,\"precision\":321.45}";
			var actual = update.ToJson(false, true);

			Assert.AreEqual(expected, actual);

			update.Page = 4;
			expected = "{\"filter\":\"\",\"order\":\"\",\"page\":4,\"perPage\":12,\"precision\":321.45}";
			actual = update.ToRawJson(false, true);

			Assert.AreEqual(expected, actual);
		}

		#endregion
	}
}