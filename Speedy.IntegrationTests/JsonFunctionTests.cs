#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.EntityFramework;
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.UnitTests;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests;

[TestClass]
public class JsonFunctionTests : SpeedyTest
{
	#region Methods

	[TestMethod]
	public void JsonConvertVersion()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"Major\":1,\"Minor\":2,\"Build\":3,\"Revision\":4}" });
				database.SaveChanges();

				var query = database
					.LogEvents
					// Server side query filter
					.Where(x => Json.ToInt32(x.Message, "$.Major") == 1)
					.ToList()
					// Client side (after to list), data convert, will not work because SQL, Sqlite cannot use this
					//.Select(x => (Version) DatabaseFunctions.JsonConvert(x.Message, "$", typeof(Version).FullName))
					.Select(x => x.Message.FromJson<Version>())
					.AsQueryable();

				var expected = new[] { new Version(1, 2, 3, 4) };
				var actual = query.ToArray();
				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void JsonNumberNullableShouldConvertToNull()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"Age\": [32,43] }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Street\": \"123 Main Street\", \"StreetNumber\": 123 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\", \"LastName\": \"Franks\", \"Age\": 45 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Name\": \"Carl Franks\", \"Age\": 32 }" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Select(x => Json.ToNullableInt32(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}


				var expected = new int?[] { null, null, 45, 32 };
				var actual = query.ToArray();
				actual.DumpJson();

				//AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void JsonString()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\" }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Fred\" }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\" }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Carl\" }" });
				database.SaveChanges();

				var query = database.LogEvents
					.Select(x => Json.GetValue(x.Message, "$.FirstName"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob", "Fred", "Ann", "Carl" };
				var actual = query.ToArray();
				AreEqual(expected, actual);

				query = database.LogEvents
					.OrderBy(x => Json.GetValue(x.Message, "$.FirstName"))
					.Select(x => Json.GetValue(x.Message, "$.FirstName"));

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				expected = new[] { "Ann", "Bob", "Carl", "Fred" };
				actual = query.ToArray();
				AreEqual(expected, actual);

				actual.DumpJson();
			});
	}

	[TestMethod]
	public void JsonStringOfDifferentStructure()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"Age\": 21 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Street\": \"123 Main Street\", \"StreetNumber\": 123 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\", \"Age\": 45 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Carl\", \"Age\": 32 }" });
				database.SaveChanges();

				var query = database.LogEvents
					.Select(x => Json.GetValue(x.Message, "$.FirstName"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob", null, "Ann", "Carl" };
				var actual = query.ToArray();
				AreEqual(expected, actual);

				query = database.LogEvents
					.OrderBy(x => Json.GetValue(x.Message, "$.FirstName"))
					.Select(x => Json.GetValue(x.Message, "$.FirstName"));

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				expected = new[] { null, "Ann", "Bob", "Carl" };
				actual = query.ToArray();
				AreEqual(expected, actual);

				actual.DumpJson();
			});
	}

	[TestMethod]
	public void JsonStringOfDifferentStructureShouldFilter()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"Age\": 21 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Street\": \"123 Main Street\", \"StreetNumber\": 123 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\", \"Age\": 45 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Carl\", \"Age\": 32 }" });
				database.SaveChanges();

				var query = database.LogEvents
					.Select(x => Json.GetValue(x.Message, "$.FirstName"))
					.Where(x => (x != null) && (x.Length <= 3));

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob", "Ann" };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToBoolean()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": true, \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": false, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Where(x => Json.ToBoolean(x.Message, "$.Started"))
					.Select(x => x.Message)
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "{\"Started\": true, \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" };
				var actual = query.ToArray();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToDateTime()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.SaveChanges();

				var until = new DateTime(2023, 01, 01);
				var query = database
					.LogEvents
					.Where(x => Json.ToDateTime(x.Message, "$.StartedOn") >= until)
					.Select(x => x.Message)
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "{\"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" };
				var actual = query.ToArray();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToInt32()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"Age\": 21 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Fred\", \"Age\": [32,43] }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Street\": \"123 Main Street\", \"StreetNumber\": 123 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\", \"LastName\": \"Franks\", \"Age\": 45 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Name\": \"Carl Franks\", \"Age\": 32 }" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Where(x => Json.ToInt32(x.Message, "$.Age") > 30)
					.Select(x => Json.ToInt32(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

				AreEqual(expected, actual);
			});
	}

	#endregion
}