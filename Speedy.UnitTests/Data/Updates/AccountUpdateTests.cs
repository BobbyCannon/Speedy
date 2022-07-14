#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Data.SyncApi;
using Speedy.Data.Updates;
using Speedy.Exceptions;
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
			Assert.AreEqual("{\"Id\":1,\"Name\":\"Testing\",\"SyncId\":\"52be31a3-ea29-45a3-90c6-2f80392bcfbc\"}", update.ToJson());

			update.ExcludedProperties.Add(nameof(Account.SyncId));
			var actual = (Account)update.GetInstance();
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);
			Assert.AreEqual("{\"Id\":1,\"Name\":\"Testing\"}", update.ToJson());
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

			var actual = (Account)update.GetInstance();
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

			var actual = (Account)update.GetInstance();
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Parse("52BE31A3-EA29-45A3-90C6-2F80392BCFBC"), actual.SyncId);

			actual = new Account();
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(null, actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);

			update.Apply(actual);
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Parse("52BE31A3-EA29-45A3-90C6-2F80392BCFBC"), actual.SyncId);

			update.IncludedProperties.Add(nameof(Account.Name));
			actual = (Account)update.GetInstance();
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual("Testing", actual.Name);
			Assert.AreEqual(Guid.Empty, actual.SyncId);
		}

		[TestMethod]
		public void MissingMembers()
		{
			var scenarios = new[] { "{}", "{ }", " { }", " { } " };

			foreach (var scenario in scenarios)
			{
				var settings = new JsonSerializerSettings();
				settings.Converters.Add(new PartialUpdateConverter());

				var update = JsonConvert.DeserializeObject<AccountUpdate>(scenario, settings);
				Assert.IsNotNull(update);
				Assert.AreEqual(0, update.Updates.Count);
				Assert.AreEqual("", string.Join(",", update.Updates.Keys));

				TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "Name is required but was not provided.");
			}
		}

		[TestMethod]
		public void ValidateMembers()
		{
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new PartialUpdateConverter());

			var update = JsonConvert.DeserializeObject<AccountUpdate>("{ \"Name\": \"a\" }", settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual("Name", string.Join(",", update.Updates.Keys));
			update.Validate();

			update = JsonConvert.DeserializeObject<AccountUpdate>("{ \"Name\": null }", settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual("Name", string.Join(",", update.Updates.Keys));
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "Name is null.");

			update = JsonConvert.DeserializeObject<AccountUpdate>("{ \"Name\": \"\" }", settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual("Name", string.Join(",", update.Updates.Keys));
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(),
				"Name must be between 1 and 5 characters in length."
			);

			update = JsonConvert.DeserializeObject<AccountUpdate>("{ \"Name\": \"123456\" }", settings);
			Assert.IsNotNull(update);
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual("Name", string.Join(",", update.Updates.Keys));
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(),
				"Name must be between 1 and 5 characters in length."
			);
		}

		#endregion
	}
}