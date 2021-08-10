#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Data.Updates;

#endregion

namespace Speedy.UnitTests.Data.Updates
{
	[TestClass]
	public class AccountUpdateTests
	{
		#region Methods

		[TestMethod]
		public void ExcludedPropertiesShouldNotBeAcceptedFromJson()
		{
			var data = "{ \"Id\": 1, \"Name\": \"Testing\", \"SyncId\": \"52BE31A3-EA29-45A3-90C6-2F80392BCFBC\" }";
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new PartialUpdateConverter());

			var update = JsonConvert.DeserializeObject<AccountUpdate>(data, settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(1, update.Updates.Count);

			var actual = update.GetInstance();
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);
		}

		#endregion
	}
}