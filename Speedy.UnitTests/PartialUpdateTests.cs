#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Data.SyncApi;
using Speedy.Data.Updates;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Sync;
using Speedy.Validation;
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
			var scenarios = new (string data, Action<MyClass> before, Action<MyClass, PartialUpdate<MyClass>> after)[]
			{
				(
					"{ \"Age\":21 }",
					before =>
					{
						Assert.IsNull(before.Name);
						Assert.AreEqual(0, before.Age);
					},
					(after, update) =>
					{
						Assert.AreEqual(1, update.Updates.Count);
						Assert.IsNull(after.Name);
						Assert.AreEqual(21, after.Age);
						Assert.AreEqual(0, after.Id);
						Assert.AreEqual(Guid.Empty, after.SyncId);
					}
				),
				(
					"{ \"age\":21 }",
					before =>
					{
						Assert.IsNull(before.Name);
						Assert.AreEqual(0, before.Age);
					},
					(after, update) =>
					{
						Assert.AreEqual(1, update.Updates.Count);
						Assert.IsNull(after.Name);
						Assert.AreEqual(21, after.Age);
						Assert.AreEqual(0, after.Id);
						Assert.AreEqual(Guid.Empty, after.SyncId);
					}
				),
				(
					"{ \"age\":\"aoeu\" }",
					before =>
					{
						Assert.IsNull(before.Name);
						Assert.AreEqual(0, before.Age);
					},
					(after, update) =>
					{
						Assert.AreEqual(0, update.Updates.Count);
						Assert.IsNull(after.Name);
						Assert.AreEqual(0, after.Age);
						Assert.AreEqual(0, after.Id);
						Assert.AreEqual(Guid.Empty, after.SyncId);
					}
				)
			};

			foreach (var scenario in scenarios)
			{
				var update = PartialUpdate.FromJson<MyClass>(scenario.data);
				var actual = new MyClass();
				scenario.before(actual);
				update.Apply(actual);
				scenario.after(actual, update);
			}
		}

		[TestMethod]
		public void ApplyShouldNotChangeExactObject()
		{
			var count = 0;
			var test = new NonBindable { Name = "Fred" };
			test.PropertyChanged += (_, _) => count++;
			Assert.AreEqual(0, count);

			var update = new PartialUpdate<NonBindable>();
			update.Set(test);

			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual(0, count);

			update.Apply(test);
			Assert.AreEqual("Fred", test.Name);
			Assert.AreEqual(0, count);
		}

		[TestMethod]
		public void DataTypes()
		{
			(object expected, string value)[] scenarios =
			{
				(new Version(1, 2, 3, 4), "1.2.3.4"),
				(new TimeSpan(1, 8, 23, 45, 999), "1.08:23:45.9990000"),
				(new DateTime(2022, 07, 16, 05, 19, 45), "07/16/2022 05:19:45 AM"),
				(42, "42"),
				(43L, "43")
			};

			foreach (var scenario in scenarios)
			{
				var update = new PartialUpdate();
				update.AddOrUpdate("t", scenario.value);
				Assert.AreEqual(scenario.expected, update.Get("t", scenario.expected.GetType()));
			}
		}

		[TestMethod]
		public void EmptyJson()
		{
			var scenarios = new[] { "{}", "[]", " { } ", " [ ] ", null, "", " ", "[{}]" };

			foreach (var scenario in scenarios)
			{
				$"\"{scenario.Escape()}\"".Dump();

				var update = (PartialUpdate) PartialUpdate.FromJson<MyClass>(scenario);
				var actual = new MyClass();
				Assert.IsNull(actual.Name);
				update.Apply(actual);
				Assert.IsNull(actual.Name);

				update = PartialUpdate.FromJson(scenario, typeof(MyClass));
				actual = new MyClass();
				Assert.IsNull(actual.Name);
				update.Apply(actual);
				Assert.IsNull(actual.Name);
			}
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
				builder.AppendLine($"{{ entities[{i}], {entities[i].ToJson().Escape()} }},");
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
		public void EnumTest()
		{
			var json = "{ \"Level\": 42 }";
			var update = PartialUpdate.FromJson<MyClass>(json);
			update.Validate(x => x.Level)
				.HasValidValue();

			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "Level does not contain a valid value.");

			json = "{ \"Level\": 0 }";
			update = PartialUpdate.FromJson<MyClass>(json);
			update.Validate();

			json = "{ \"Level\": 5 }";
			update = PartialUpdate.FromJson<MyClass>(json);
			update.Validate();

			var partial = new PartialUpdate();
			partial.AddOrUpdate("LogLevel", ((int) LogLevel.Information).ToString());
			Assert.AreEqual(LogLevel.Information, partial.Get<LogLevel>("LogLevel"));
		}

		[TestMethod]
		public void ExcludedProperties()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var update = PartialUpdate.FromJson(json, typeof(MyClass));

			// Ensure the updates exists
			Assert.AreEqual(4, update.Updates.Count);
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Id)));
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.ModifiedOn)));
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Age)));
			Assert.IsTrue(update.Updates.Any(x => x.Key == nameof(MyClass.Name)));

			// Exclude two of the updates
			update.Options.ExcludedProperties.AddRange(nameof(MyClass.Id), nameof(MyClass.ModifiedOn));

			// Ensure these exclusions are ignored on "Apply"
			var actual = new MyClass();
			update.Apply(actual);
			Assert.AreEqual(21, actual.Age);
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(DateTime.MinValue, actual.ModifiedOn);
			Assert.AreEqual("foo bar", actual.Name);
		}

		[TestMethod]
		public void ExplicitStringConversion()
		{
			var scenarios = new (string value, PartialUpdate expected)[]
			{
				("{\"foo\":\"bar\",\"age\":21}",
					new PartialUpdate
					{
						Updates = { { "foo", new PartialUpdateValue("foo", "bar") }, { "age", new PartialUpdateValue("age", 21) } }
					}
				),
				("?foo=bar&age=21",
					new PartialUpdate
					{
						Updates = { { "foo", new PartialUpdateValue("foo", "bar") }, { "age", new PartialUpdateValue("age", "21") } }
					}
				),
			};

			foreach (var scenario in scenarios)
			{
				var actual = (PartialUpdate) scenario.value;
				TestHelper.AreEqual(scenario.expected, actual);
			}
		}

		[TestMethod]
		public void ExtraPropertiesIgnored()
		{
			var json = "{ \"foo\": \"bar\", \"Id\": 42 }";
			var update = PartialUpdate.FromJson<Account>(json);
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual(true, update.Updates.ContainsKey("Id"));

			var actual = update.GetInstance();
			Assert.AreEqual(42, actual.Id);
		}

		[TestMethod]
		public void GetWithDefault()
		{
			var request = new PartialUpdate<Account>();

			// Should return 23 because an update does not exist
			var actual = request.Get(x => x.Id, 23);
			Assert.AreEqual(23, actual);

			request.Set(x => x.Id, 25);

			// Should return 25 instead
			actual = request.Get(x => x.Id, 23);
			Assert.AreEqual(25, actual);
		}

		[TestMethod]
		public void IncludeProperties()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var options = new PartialUpdateOptions();
			options.IncludedProperties.AddRange(nameof(MyClass.Age), nameof(MyClass.Name));

			var update = PartialUpdate.FromJson(json, options);

			// Ensure these updates do exists
			Assert.AreEqual(2, update.Updates.Count);
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
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>(null).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>(" ").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("\t").Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson<MyClass>("[]").Updates.Count);
			TestHelper.ExpectedException<JsonReaderException>(() => PartialUpdate.FromJson<MyClass>("1"), "Error reading JObject from JsonReader");

			var type = typeof(MyClass);
			Assert.AreEqual(0, PartialUpdate.FromJson(json, type).Updates.Count);
			Assert.AreEqual(0, PartialUpdate.FromJson(null, type).Updates.Count);
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
		public void PartialUpdateAdd()
		{
			var update = new PartialUpdate<Account>();
			Assert.AreEqual(0, update.Updates.Count);
			update.Set(nameof(Account.Name), "Fred");
			Assert.AreEqual("{\"Name\":\"Fred\"}", update.ToRawJson());
			Assert.AreEqual("{\"Name\":\"Fred\"}", update.ToRawJson());

			update = new PartialUpdate<Account>();
			Assert.AreEqual(0, update.Updates.Count);
			update.Set(x => x.Name, "Fred");
			Assert.AreEqual("{\"Name\":\"Fred\"}", update.ToRawJson());
			Assert.AreEqual("{\"Name\":\"Fred\"}", update.ToRawJson());

			update = new PartialUpdate<Account>();
			TestHelper.ExpectedException<SpeedyException>(
				() => update.Set(nameof(Account.Name), 21),
				"The property type does not match the values type."
			);

			var accountUpdate = new AccountUpdate();
			Assert.AreEqual(0, accountUpdate.Updates.Count);

			accountUpdate.Set(nameof(Account.Name), "Fred");
			Assert.AreEqual(1, accountUpdate.Updates.Count);
			Assert.AreEqual("Name", accountUpdate.Updates["Name"].Name);
			Assert.AreEqual("Fred", accountUpdate.Updates["Name"].Value);
			Assert.AreEqual("Fred", accountUpdate.Get(nameof(Account.Name)));
			Assert.AreEqual("Fred", accountUpdate.Get<string>(nameof(Account.Name)));
		}

		[TestMethod]
		public void PartialUpdateApply()
		{
			var update = new PartialUpdate<Account>();
			update.Set(x => x.Name, "Fred");
			var account = new Account();
			Assert.AreEqual(null, account.Name);

			update.Apply(account);
			Assert.AreEqual("Fred", account.Name);

			var accountUpdate = new AccountUpdate();
			Assert.AreEqual(0, accountUpdate.Updates.Count);

			accountUpdate.Set(nameof(Account.Name), "Fred");
			Assert.AreEqual(1, accountUpdate.Updates.Count);
			Assert.AreEqual("Name", accountUpdate.Updates["Name"].Name);
			Assert.AreEqual("Fred", accountUpdate.Updates["Name"].Value);
		}

		[TestMethod]
		public void PartialUpdateFromJson()
		{
			var account = new AccountEntity();
			var json = @"{""Name"":""Bob""}";
			var update = PartialUpdate.FromJson<AccountEntity>(json);
			Assert.AreEqual(false, account.HasChanges());

			update.Apply(account);
			Assert.AreEqual(true, account.HasChanges());
			Assert.AreEqual(json, update.ToRawJson());
		}

		[TestMethod]
		public void PartialUpdateGetInstance()
		{
			var update = new PartialUpdate<Account>();
			update.Set(nameof(Account.Name), "Fred");
			var account = update.GetInstance();
			Assert.AreEqual("Fred", account.Name);
			Assert.AreEqual("{\"Name\":\"Fred\"}", update.ToRawJson());
		}

		[TestMethod]
		public void Remove()
		{
			var update = new PartialUpdate<Account>();
			update.Set(nameof(Account.Name), "Fred");
			Assert.AreEqual(1, update.Updates.Count);
			Assert.AreEqual("Name", update.Updates["Name"].Name);

			update.Remove(x => x.Name);
			Assert.AreEqual(0, update.Updates.Count);
		}

		[TestMethod]
		public void SetUsingObject()
		{
			var expected = new MyClass
			{
				Age = 21,
				Name = "John Doe",
				Level = LogLevel.Error,
				Id = 4,
				IsDeleted = true,
				CreatedOn = new DateTime(2022, 05, 03, 09, 06, 12, DateTimeKind.Utc),
				ModifiedOn = new DateTime(2022, 05, 03, 09, 06, 13, DateTimeKind.Utc),
				SyncId = Guid.Parse("EDFA8A18-B693-4B61-BDE2-EEB727398556")
			};

			var partialUpdate = new PartialUpdate<MyClass>();
			Assert.AreEqual(0, partialUpdate.Updates.Count);

			partialUpdate.Set(expected);
			Assert.AreEqual(8, partialUpdate.Updates.Count);
			Assert.AreEqual(expected.Age, partialUpdate.Get(x => x.Age));
			Assert.AreEqual(expected.Name, partialUpdate.Get(x => x.Name));
			Assert.AreEqual(expected.Level, partialUpdate.Get(x => x.Level));
			Assert.AreEqual(expected.Id, partialUpdate.Get(x => x.Id));
			Assert.AreEqual(expected.IsDeleted, partialUpdate.Get(x => x.IsDeleted));
			Assert.AreEqual(expected.CreatedOn, partialUpdate.Get(x => x.CreatedOn));
			Assert.AreEqual(expected.ModifiedOn, partialUpdate.Get(x => x.ModifiedOn));
			Assert.AreEqual(expected.SyncId, partialUpdate.Get(x => x.SyncId));
		}

		[TestMethod]
		public void ToRawJson()
		{
			var update = new PartialUpdate();
			update.AddOrUpdate("Name", "Bob");
			update.AddOrUpdate("Age", 21);

			var expected = "{\"Age\":21,\"Name\":\"Bob\"}";
			Assert.AreEqual(expected, update.ToRawJson());
		}

		[TestMethod]
		public void Validate()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var rangeMessage = "Name must be between 1 to 5 characters.";
			var requiredMessage = "Name is required";

			var update = json.FromJson<PartialUpdate>();
			update.Validate<MyClass, string>(x => x.Name)
				.HasMinMaxRange(1, 5, rangeMessage)
				.IsNotNullOrWhitespace();

			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), rangeMessage);

			json = "{ \"Age\":21, \"Id\": 42, \"Name\": null, \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			update = json.FromJson<PartialUpdate>();
			update.Validate<MyClass, string>(x => x.Name)
				.HasMinMaxRange(1, 5, rangeMessage)
				.IsNotNullOrWhitespace()
				.IsRequired(requiredMessage);

			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "Name is null or whitespace.");
		}

		[TestMethod]
		public void ValidateOptionalShouldNotThrow()
		{
			var scenarios = new[] { "{}", "[]", "" };
			foreach (var json in scenarios)
			{
				var update = PartialUpdate.FromJson<MyClass>(json);
				update.Validate(x => x.Name)
					.IsOptional()
					.HasMinMaxRange(1, 5, "Name must be between 1 to 5 characters.");

				update.Validate();
			}
		}

		[TestMethod]
		public void ValidateWithGeneric()
		{
			var json = "{ \"Age\":21, \"Id\": 42, \"Name\":\"foo bar\", \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			var rangeMessage = "Name must be between 1 to 5 characters.";
			var requiredMessage = "Name is required";

			var update = PartialUpdate.FromJson<MyClass>(json);
			update.Validate(x => x.Name)
				.HasMinMaxRange(1, 5, rangeMessage)
				.IsNotNullOrWhitespace()
				.IsRequired(requiredMessage);

			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), rangeMessage);

			json = "{ \"Age\":21, \"Id\": 42, \"Name\": null, \"ModifiedOn\": \"2021-07-15T09:08:12Z\" }";
			update = PartialUpdate.FromJson<MyClass>(json);
			update.Validate(x => x.Name)
				.HasMinMaxRange(1, 5, rangeMessage)
				.IsNotNullOrWhitespace()
				.IsRequired(requiredMessage);

			TestHelper.ExpectedException<ValidationException>(() => update.Validate(), "Name is null or whitespace.");
		}

		#endregion

		#region Classes

		public class MyClass : SyncModel<long>
		{
			#region Properties

			public int Age { get; set; }

			public override long Id { get; set; }

			public LogLevel Level { get; set; }

			public string Name { get; set; }

			#endregion
		}

		public class NonBindable : INotifyPropertyChanged
		{
			#region Fields

			private string _name;

			#endregion

			#region Properties

			public string Name
			{
				get => _name;
				set
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}

			#endregion

			#region Methods

			protected virtual void OnPropertyChanged(string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			#endregion

			#region Events

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion
		}

		#endregion
	}
}