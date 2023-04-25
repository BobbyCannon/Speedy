#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.EntityFramework;
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.UnitTests;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests;

[TestClass]
public class JsonFunctionTests : SpeedyTest
{
	#region Methods

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
					.Select(x => Json.Value(x.Message, "$.FirstName"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob", null, "Ann", "Carl" };
				var actual = query.ToArray();
				AreEqual(expected, actual);

				query = database.LogEvents
					.OrderBy(x => Json.Value(x.Message, "$.FirstName"))
					.Select(x => Json.Value(x.Message, "$.FirstName"));

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
	public void ToBoolean()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": true, \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": false, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": null, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": \"NotBoolean\", \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Where(x => Json.ToNullableBoolean(x.Message, "$.Started") == true)
					.Select(x => Json.ToNullableBoolean(x.Message, "$.Started"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new bool?[] { true };
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
	public void ToInt16()
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
					.Where(x => Json.ToInt16(x.Message, "$.Age") > 30)
					.Select(x => Json.ToInt16(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new short[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

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

	[TestMethod]
	public void ToInt64()
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
					.Where(x => Json.ToInt64(x.Message, "$.Age") > 30)
					.Select(x => Json.ToInt64(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new long[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableBoolean()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": true, \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": false, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": 0, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"Started\": null, \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Where(x => Json.ToNullableBoolean(x.Message, "$.Started") == true)
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
	public void ToNullableDateTime()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				database.LogEvents.Add(new LogEventEntity { Message = "{\"StartedOn\": \"2023-04-21T15:34:33.2187670Z\"}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"StartedOn\": null}" });
				database.LogEvents.Add(new LogEventEntity { Message = "{\"StartedOn\": \"2022-04-21T15:34:33.2187670Z\"}" });
				database.SaveChanges();

				var until = new DateTime(2023, 01, 01);
				var query = database
					.LogEvents
					.Where(x => Json.ToNullableDateTime(x.Message, "$.StartedOn") >= until)
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
	public void ToNullableInt16()
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
					.Select(x => Json.ToNullableInt16(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new short?[] { 0, null, 45, 32 }
					: new short?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableInt32()
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

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new int?[] { 0, null, 45, 32 }
					: new int?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableInt64()
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
					.Select(x => Json.ToNullableInt64(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new long?[] { 0, null, 45, 32 }
					: new long?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableUInt16()
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
					.Select(x => Json.ToNullableUInt16(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new ushort?[] { 0, null, 45, 32 }
					: new ushort?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				//AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableUInt32()
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
					.Select(x => Json.ToNullableUInt32(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new uint?[] { 0, null, 45, 32 }
					: new uint?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableUInt64()
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
					.Select(x => Json.ToNullableUInt64(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				// todo: Sqlite is different, need to see if we can correct?
				var expected = database.GetDatabaseType() == DatabaseType.Sqlite
					? new ulong?[] { 0, null, 45, 32 }
					: new ulong?[] { null, null, 45, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToUInt16()
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
					.Where(x => Json.ToUInt16(x.Message, "$.Age") > 30)
					.Select(x => Json.ToUInt16(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new ushort[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToUInt32()
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
					.Where(x => Json.ToUInt32(x.Message, "$.Age") > 30)
					.Select(x => Json.ToUInt32(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new uint[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToUInt64()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = provider.GetDatabase();
				Console.WriteLine(database.GetType().Name);

				if (database.GetDatabaseType() == DatabaseType.Sqlite)
				{
					Console.WriteLine("\tNot Supported");
					return;
				}

				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"Age\": 21 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Fred\", \"Age\": [32,43] }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Street\": \"123 Main Street\", \"StreetNumber\": 123 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Ann\", \"LastName\": \"Franks\", \"Age\": 45 }" });
				database.LogEvents.Add(new LogEventEntity { Message = "{ \"Name\": \"Carl Franks\", \"Age\": 32 }" });
				database.SaveChanges();

				var query = database
					.LogEvents
					.Where(x => Json.ToUInt64(x.Message, "$.Age") > 30)
					.Select(x => Json.ToUInt64(x.Message, "$.Age"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new ulong[] { 45, 32 };
				var actual = query.ToArray();
				actual.Dump();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void Value()
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
					.Select(x => Json.Value(x.Message, "$.FirstName"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob", "Fred", "Ann", "Carl" };
				var actual = query.ToArray();
				AreEqual(expected, actual);

				query = database.LogEvents
					.OrderBy(x => Json.Value(x.Message, "$.FirstName"))
					.Select(x => Json.Value(x.Message, "$.FirstName"));

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
	public void ValueOfDifferentStructureShouldFilter()
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
					.Select(x => Json.Value(x.Message, "$.FirstName"))
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

	#endregion
}