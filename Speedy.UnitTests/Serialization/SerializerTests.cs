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
			var expected = "{\"$id\":\"1\",\"age\":21,\"createdOn\":\"2020-02-17T12:12:45Z\",\"id\":42,\"isDeleted\":false,\"modifiedOn\":\"2020-02-17T12:12:45Z\",\"name\":\"John Doe\",\"percent\":1.23,\"syncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"testEnum\":1}";
			var actual = data.ToJson(camelCase: true);
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationConvertEnumsToString()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":\"Second\"}";
			var actual = data.ToJson(convertEnumsToString: true);
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationDefaults()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1}";
			var actual = data.ToJson();
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationIgnoreGlobalMember()
		{
			var data = GetTestClass(update: x => x.Age = 22);
			var settings = new SerializerSettings();
			settings.Ignore(nameof(ISyncEntity.CreatedOn), nameof(ISyncEntity.ModifiedOn), "Id", nameof(ISyncEntity.SyncId), nameof(ISyncEntity.IsDeleted));
			var expected = "{\"$id\":\"1\",\"Age\":22,\"Name\":\"John Doe\",\"Percent\":1.23,\"TestEnum\":1}";
			var actual = data.ToJson(settings);
			//actual.Dump();
			Assert.AreEqual(expected, actual);

			settings.Reset();
			expected = "{\"$id\":\"1\",\"Age\":22,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1}";
			actual = data.ToJson(settings);
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationIgnoreNullValues()
		{
			var data = GetTestClass(null, 22);
			var expected = "{\"$id\":\"1\",\"Age\":22,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1}";
			var actual = data.ToJson(ignoreNullValues: true);
			//actual.Dump();
			Assert.IsNull(data.Name);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationIgnoreReadOnly()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Percent\":1.23,\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1}";
			var actual = data.ToJson(ignoreReadOnly: true);
			//actual.Dump();
			Assert.AreEqual("John Doe", data.Name);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationIgnoreVirtuals()
		{
			var data = GetTestClass();
			var expected = "{\"$id\":\"1\",\"Age\":21,\"CreatedOn\":\"2020-02-17T12:12:45Z\",\"Id\":42,\"IsDeleted\":false,\"ModifiedOn\":\"2020-02-17T12:12:45Z\",\"Name\":\"John Doe\",\"SyncId\":\"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\"TestEnum\":1}";
			var actual = data.ToJson(ignoreVirtuals: true);
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void SerializationIndented()
		{
			var data = GetTestClass();
			var expected = "{\r\n  \"$id\": \"1\",\r\n  \"Age\": 21,\r\n  \"CreatedOn\": \"2020-02-17T12:12:45Z\",\r\n  \"Id\": 42,\r\n  \"IsDeleted\": false,\r\n  \"ModifiedOn\": \"2020-02-17T12:12:45Z\",\r\n  \"Name\": \"John Doe\",\r\n  \"Percent\": 1.23,\r\n  \"SyncId\": \"a7dd0efd-37e8-4777-bdda-5cb296e74806\",\r\n  \"TestEnum\": 1\r\n}";
			var actual = data.ToJson(true);
			//actual.Dump();
			Assert.AreEqual(expected, actual);
		}

		private static SerializerTestClass GetTestClass(string name = "John Doe", int age = 21, Action<SerializerTestClass> update = null)
		{
			var response = new SerializerTestClass(name, age)
			{
				CreatedOn = new DateTime(2020, 02, 17, 12, 12, 45, DateTimeKind.Utc),
				Id = 42,
				SyncId = Guid.Parse("A7DD0EFD-37E8-4777-BDDA-5CB296E74806"),
				IsDeleted = false,
				ModifiedOn = new DateTime(2020, 02, 17, 12, 12, 45, DateTimeKind.Utc),
				Percent = 1.23,
				TestEnum = SerializerTestEnum.Second
			};

			update?.Invoke(response);
			return response;
		}

		#endregion

		#region Classes

		private class SerializerTestClass : SyncModel<int>
		{
			#region Constructors

			public SerializerTestClass(string name, int age)
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