#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Data.SyncApi;
using Speedy.Data.Updates;
using Speedy.Serialization.Converters;

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
			Assert.AreEqual(3, update.Updates.Count);
			Assert.AreEqual("Id,Name,SyncId", string.Join(",", update.Updates.Keys));

			update.Options.ExcludedProperties.Add(nameof(AccountUpdate.SyncId));
			var actual = (Account) update.GetInstance();
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);
		}

		[TestMethod]
		public void GetInstanceShouldAcceptAllFromJson()
		{
			var data = "{ \"Id\": 1, \"Name\": \"Testing\", \"SyncId\": \"52BE31A3-EA29-45A3-90C6-2F80392BCFBC\" }";
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new PartialUpdateConverter());

			var update = JsonConvert.DeserializeObject<AccountUpdate>(data, settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(3, update.Updates.Count);
			Assert.AreEqual("Id,Name,SyncId", string.Join(",", update.Updates.Keys));

			var actual = (Account) update.GetInstance();
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Parse("52BE31A3-EA29-45A3-90C6-2F80392BCFBC"), actual.SyncId);
		}

		[TestMethod]
		public void IncludedPropertiesShouldOnlyBeAcceptedFromJson()
		{
			var data = "{ \"Id\": 1, \"Name\": \"Testing\", \"SyncId\": \"52BE31A3-EA29-45A3-90C6-2F80392BCFBC\" }";
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new PartialUpdateConverter());

			var update = JsonConvert.DeserializeObject<AccountUpdate>(data, settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(3, update.Updates.Count);
			Assert.AreEqual("Id,Name,SyncId", string.Join(",", update.Updates.Keys));

			update.Options.IncludedProperties.Add(nameof(AccountUpdate.Name));
			var actual = (Account) update.GetInstance();
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);
		}

		#endregion
	}
}