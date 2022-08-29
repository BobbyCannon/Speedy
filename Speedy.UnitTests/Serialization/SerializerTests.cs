#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Serialization;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Serialization
{
	[TestClass]
	public class SerializerTests
	{
		#region Methods

		[TestMethod]
		public void SerializationCamelCase()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"age\":21,\"createdOn\":\"2020-02-17T12:12:45Z\",\"id\":42,\"isDeleted\":false,\"modifiedOn\":\"2020-02-17T12:12:45Z\",\"name\":\"John Doe\",\"percent\":1.23,\"syncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"testEnum\":1,\"version\":\"1.2.3.4\"}";
			ToFromJson(data, expected, x => x.CamelCase = true);
		}

		[TestMethod]
		public void SerializationConvertEnumsToString()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":\"Second\",\"Version\":\"1.2.3.4\"}";
			ToFromJson(data, expected, x => x.ConvertEnumsToString = true);
		}

		[TestMethod]
		public void SerializationDefaults()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			ToFromJson(data, expected, _ => { });
		}

		[TestMethod]
		public void SerializationIgnoreGlobalMember()
		{
			var data = GetTestClass(update: x => x.Age = 22);
			var settings = new SerializerSettings();
			var membersToIgnore = new[] { nameof(ISyncEntity.CreatedOn), nameof(ISyncEntity.ModifiedOn), "Id", nameof(ISyncEntity.SyncId), nameof(ISyncEntity.IsDeleted) };
			settings.Ignore(membersToIgnore);
			var expected = "{\"$id\":\"1\",\"Age\":22,\"Name\":\"John Doe\",\"Percent\":1.23,\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			ToFromJson(data, expected, settings, membersToIgnore);

			settings.Reset();
			expected = "{\"$id\":\"1\",\"Age\":22,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			ToFromJson(data, expected, settings);
		}

		[TestMethod]
		public void SerializationIgnoreNullValues()
		{
			var data = GetTestClass(null, 22);
			var expected = "{\"$id\":\"1\",\"Age\":22,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			var actual = data.ToJson(ignoreNullValues: true);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.IsNull(data.Name);
			Assert.AreEqual(expected, actual);

			ToFromJson(data, expected, x => x.IgnoreNullValues = true,
				nameof(TestClass.Name));
		}

		[TestMethod]
		public void SerializationIgnoreReadOnly()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			var actual = data.ToJson(ignoreReadOnly: true);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual("John Doe", data.Name);
			Assert.AreEqual(expected, actual);

			ToFromJson(data, expected, x => x.IgnoreReadOnly = true, nameof(TestClass.Name));
		}

		[TestMethod]
		public void SerializationIgnoreVirtuals()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1,\"Version\":\"1.2.3.4\"}";
			var actual = data.ToJson(ignoreVirtuals: true);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expected, actual);

			ToFromJson(data, expected, x => x.IgnoreVirtuals = true, nameof(TestClass.Percent));
		}

		[TestMethod]
		public void SerializationIndented()
		{
			var data = GetTestClass();
			var expected = "{\r\n  \"$id\": \"1\",\r\n  \"Age\": 21,\r\n  \"CreatedOn\": \"2020-02-17T12:12:45Z\",\r\n  \"Id\": 42,\r\n  \"IsDeleted\": false,\r\n  \"ModifiedOn\": \"2020-02-17T12:12:45Z\",\r\n  \"Name\": \"John Doe\",\r\n  \"Percent\": 1.23,\r\n  \"SyncId\": \"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\r\n  \"TestEnum\": 1,\r\n  \"Version\": \"1.2.3.4\"\r\n}";
			var actual = data.ToJson(true);
			//actual.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expected, actual);

			ToFromJson(data, expected, x => x.Indented = true);
		}

		[TestMethod]
		public void ToJson()
		{
			var data = GetTestClass();
			var json = "{\r\n  \"age\": 21,\r\n  \"createdOn\": \"2020-02-17T12:12:45Z\",\r\n  \"id\": 42,\r\n  \"isDeleted\": false,\r\n  \"modifiedOn\": \"2020-02-17T12:12:45Z\",\r\n  \"name\": \"John Doe\",\r\n  \"percent\": 1.23,\r\n  \"syncId\": \"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\r\n  \"testEnum\": \"second\",\r\n  \"version\": \"1.2.3.4\"\r\n}";
			ToFromJson(data, json);
		}

		private static TestClass GetTestClass(string name = "John Doe", int age = 21, Action<TestClass> update = null)
		{
			var response = new TestClass(name, age)
			{
				CreatedOn = new DateTime(2020, 02, 17, 12, 12, 45, DateTimeKind.Utc),
				Id = 42,
				SyncId = Guid.Parse("A7DD0EFD-37E8-4777-BDDA-5CB296E74806"),
				IsDeleted = false,
				ModifiedOn = new DateTime(2020, 02, 17, 12, 12, 45, DateTimeKind.Utc),
				Percent = 1.23,
				TestEnum = SerializerTestEnum.Second,
				Version = new Version(1, 2, 3, 4)
			};

			update?.Invoke(response);
			return response;
		}

		private void ToFromJson<T>(T expectedObject, string expectedJson)
		{
			var actualJson = expectedObject.ToRawJson(true, true, convertEnumsToString: true);
			//actualJson.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expectedJson, actualJson);
			var actualObject = actualJson.FromJson<T>();
			TestHelper.AreEqual(expectedObject, actualObject);
		}

		private void ToFromJson<T>(T expectedObject, string expectedJson, SerializerSettings settings, params string[] exceptions)
		{
			var actualJson = expectedObject.ToJson(settings);
			//actualJson.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expectedJson, actualJson);
			var actualObject = actualJson.FromJson<T>(settings);
			TestHelper.AreEqual(expectedObject, actualObject, exceptions);
		}
		
		private void ToFromJson<T>(T expectedObject, string expectedJson, Action<SerializerSettings> update, params string[] exceptions)
		{
			Serializer.ResetDefaultSettings();
			update(Serializer.DefaultSettings);
			var actualJson = expectedObject.ToJson();
			//actualJson.Escape().CopyToClipboard().Dump();
			Assert.AreEqual(expectedJson, actualJson);
			var actualObject = actualJson.FromJson<T>();
			TestHelper.AreEqual(expectedObject, actualObject, exceptions);
		}

		#endregion

		#region Classes

		private class TestClass : SyncModel<int>
		{
			#region Constructors

			public TestClass(string name, int age)
			{
				Name = name;
				Age = age;
			}

			#endregion

			#region Properties

			public int Age { get; set; }

			public override int Id { get; set; }

			public string Name { get; }

			public virtual double Percent { get; set; }

			public SerializerTestEnum TestEnum { get; set; }

			public Version Version { get; set; }

			#endregion
		}

		#endregion

		#region Enumerations

		private enum SerializerTestEnum
		{
			First,
			Second,
			Third
		}

		#endregion
	}
}