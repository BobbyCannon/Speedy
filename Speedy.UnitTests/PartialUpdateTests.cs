﻿#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Sync;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class PartialUpdateTests
	{
		#region Methods

		[TestMethod]
		public void AgeTest()
		{
			var json = "{ \"Age\":21 }";
			var update = PartialUpdate.FromJson<MyClass>(json);
			var actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.IsNull(actual.Name);
			Assert.AreEqual(21, actual.Age);
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(Guid.Empty, actual.SyncId);

			json = "{ \"age\":21 }";
			update = PartialUpdate.FromJson<MyClass>(json);
			actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.IsNull(actual.Name);
			Assert.AreEqual(21, actual.Age);

			json = "{ \"age\":\"aoeu\" }";
			update = PartialUpdate.FromJson<MyClass>(json);
			Assert.AreEqual(0, update.Updates.Count);
			actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.IsNull(actual.Name);
			Assert.AreEqual(0, actual.Age);
		}

		[TestMethod]
		public void EmptyJson()
		{
			var json = "{}";
			var update = PartialUpdate.FromJson<MyClass>(json);
			var actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.IsNull(actual.Name);

			json = "[]";
			update = PartialUpdate.FromJson<MyClass>(json);
			actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.IsNull(actual.Name);
		}

		[TestMethod]
		public void EntityParsing()
		{
			var entities = new ICreatedEntity[]
			{
				new AddressEntity
				{
					City = "City",
					CreatedOn = new DateTime(2021, 07, 15, 09, 08, 12, DateTimeKind.Utc),
					Id = 42,
					Line1 = "Line1",
					Line2 = "Line2",
					IsDeleted = true,
					ModifiedOn = new DateTime(2021, 07, 15, 09, 08, 13, DateTimeKind.Utc),
					Postal = "123456",
					State = "AB",
					SyncId = Guid.Parse("39AA4CBE-8FB9-4264-84E9-43E35A4CCEB2")
				},
				new AccountEntity
				{
					AddressId = 1,
					AddressSyncId = Guid.Parse("65CC7CDC-651D-4E45-9520-4E066CEA65A0"),
					CreatedOn = new DateTime(2021, 07, 15, 09, 08, 14, DateTimeKind.Utc),
					EmailAddress = "test@domain.com",
					ExternalId = "ABC-123",
					Id = 45,
					IsDeleted = true,
					LastLoginDate = new DateTime(2021, 07, 15, 09, 08, 16, DateTimeKind.Utc),
					ModifiedOn = new DateTime(2021, 07, 15, 09, 08, 15, DateTimeKind.Utc),
					Name = "Foo Bar",
					Nickname = "Hello World",
					PasswordHash = "*&%(^$&^&*#^",
					Roles = "blah,foo,yes",
					SyncId = Guid.Parse("8AE9C66E-0710-4BDE-9098-E0E767C4E1B1")
				}
			};
			var builder = new StringBuilder();
			entities.FirstOrDefault(x => x.CreatedOn == DateTime.MinValue)?.UpdateWith(new { UsageStastics = "123" });

			for (var i = 0; i < entities.Length; i++)
			{
				builder.AppendLine($"{{ entities[{i}], {entities[i].ToJson().ToLiteral()} }},");
			}

			//Clipboard.SetText(builder.ToString());
			builder.ToString().Dump();

			var expected = new Dictionary<IEntity, string>
			{
				{ entities[0], "{\"$id\":\"1\",\"Account\":null,\"AccountId\":null,\"Accounts\":[],\"AccountSyncId\":null,\"City\":\"City\",\"CreatedOn\":\"2021-07-15T09:08:12Z\",\"FullAddress\":\"Line1\\r\\nCity, AB  123456\",\"Id\":42,\"IsDeleted\":true,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddress\":null,\"LinkedAddresses\":[],\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2021-07-15T09:08:13Z\",\"Postal\":\"123456\",\"State\":\"AB\",\"SyncId\":\"39aa4cbe-8fb9-4264-84e9-43e35a4cceb2\"}" },
				{ entities[1], "{\"$id\":\"1\",\"Address\":null,\"AddressId\":1,\"AddressSyncId\":\"65cc7cdc-651d-4e45-9520-4e066cea65a0\",\"CreatedOn\":\"2021-07-15T09:08:14Z\",\"EmailAddress\":\"test@domain.com\",\"ExternalId\":\"ABC-123\",\"Groups\":[],\"Id\":45,\"IsDeleted\":true,\"LastLoginDate\":\"2021-07-15T09:08:16Z\",\"ModifiedOn\":\"2021-07-15T09:08:15Z\",\"Name\":\"Foo Bar\",\"Nickname\":\"Hello World\",\"PasswordHash\":\"*&%(^$&^&*#^\",\"Pets\":[],\"Roles\":\"blah,foo,yes\",\"SyncId\":\"8ae9c66e-0710-4bde-9098-e0e767c4e1b1\"}" }
			};

			foreach (var item in expected)
			{
				var itemType = item.Key.GetType();
				var actual = Activator.CreateInstance(itemType);
				var update = PartialUpdate.FromJson(item.Value, itemType);
				update.Apply(actual);
				TestHelper.AreEqual(item.Key, actual);
			}
		}

		[TestMethod]
		public void ExcludedProperties()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var options = new PartialUpdateOptions<MyClass>();
			options.ExcludeProperties(nameof(MyClass.Id), nameof(MyClass.ModifiedOn));
			var update = PartialUpdate.FromJson(json, options);
			Assert.AreEqual(2, update.Updates.Count);

			// Ensure these updates do not exists
			Assert.IsFalse(update.Updates.Any(x => x.Key == nameof(MyClass.Id)));
			Assert.IsFalse(update.Updates.Any(x => x.Key == nameof(MyClass.ModifiedOn)));

			// Ensure these update do exists
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Age)));
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Name)));

			// Ensure these members are ignore on "Apply"
			var actual = new MyClass();
			update.Apply(actual);
			Assert.AreEqual(21, actual.Age);
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(DateTime.MinValue, actual.ModifiedOn);
			Assert.AreEqual("foo bar", actual.Name);
		}

		[TestMethod]
		public void IncludeProperties()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var options = new PartialUpdateOptions<MyClass>();
			options.IncludeProperties(nameof(MyClass.Age), nameof(MyClass.Name));
			var update = PartialUpdate.FromJson(json, options);
			Assert.AreEqual(2, update.Updates.Count);

			// Ensure these updates do not exists
			Assert.IsFalse(update.Updates.Any(x => x.Key == nameof(MyClass.Id)));
			Assert.IsFalse(update.Updates.Any(x => x.Key == nameof(MyClass.ModifiedOn)));

			// Ensure these updates do exists
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Age)));
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Name)));

			// Ensure these members are the only ones set on "Apply"
			var actual = new MyClass();
			update.Apply(actual);
			Assert.AreEqual(21, actual.Age);
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(DateTime.MinValue, actual.ModifiedOn);
			Assert.AreEqual("foo bar", actual.Name);
		}

		[TestMethod]
		public void InvalidJson()
		{
			var json = "{ \"DoesNotExistOnMyClass\": true }";
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>(json).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>((string) null).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>(" ").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("\t").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("[]").Updates.Count);
			TestHelper.ExpectedException<JsonReaderException>(() => PartialUpdate.FromJson<MyClass>("1"), "Error reading JObject from JsonReader");

			var type = typeof(MyClass);
			Assert.AreEqual(0, PartialUpdate.FromJson(json, type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson((string) null, type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson("", type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson(" ", type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson("\t", type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson("[]", type).Updates.Count);
			TestHelper.ExpectedException<JsonReaderException>(() => PartialUpdate.FromJson("1", type), "Error reading JObject from JsonReader");
		}

		[TestMethod]
		public void NameTest()
		{
			var json = "{ \"Name\":\"foobar\" }";
			var update = PartialUpdate.FromJson<MyClass>(json);
			var actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.AreEqual("foobar", actual.Name);

			json = "{ \"name\":\"foobar\" }";
			update = PartialUpdate.FromJson<MyClass>(json);
			actual = new MyClass();
			Assert.IsNull(actual.Name);
			update.Apply(actual);
			Assert.AreEqual("foobar", actual.Name);
		}

		[TestMethod]
		public void Validate()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var options = new PartialUpdateOptions<MyClass>();
			var message = "Name must be between 1 to 5 characters.";
			options.Property(x => x.Name).HasMinMaxRange(1,5).IsRequired().Throws(message);
			var update = PartialUpdate.FromJson(json, options);
			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), message);
		}

		[TestMethod]
		public void ValidateOptionalShouldNotThrow()
		{
			var scenarios = new[] { "{}", "[]", "" };
			foreach (var json in scenarios)
			{
				var options = new PartialUpdateOptions<MyClass>();
				var message = "Name must be between 1 to 5 characters.";
				options.Property(x => x.Name).HasMinMaxRange(1,5).IsRequired(false).Throws(message);
				var update = PartialUpdate.FromJson(json, options);
				update.Validate();
			}
		}

		#endregion

		#region Classes

		public class MyClass : SyncModel<long>
		{
			#region Properties

			public int Age { get; set; }

			public override long Id { get; set; }

			public string Name { get; set; }

			#endregion
		}

		#endregion
	}
}