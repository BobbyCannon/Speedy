#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.EntityFramework;
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.UnitTests;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests;

[TestClass]
public class JsonFunctionTests : SpeedyTest
{
	#region Methods

	[TestMethod]
	public void ToBoolean()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
				var query = database
					.LogEvents
					.Where(x => Json.ToNullableBoolean(x.Message, "$.Started") == true)
					.Select(x => Json.ToNullableBoolean(x.Message, "$.Started"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new bool?[] { true, true };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToDateTime()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
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

				var expected = new[] { "{ \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\", \"Started\": true }" };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToInt16()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
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
				using var database = GenerateData(provider);
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
				using var database = GenerateData(provider);
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
				using var database = GenerateData(provider);
				var query = database
					.LogEvents
					.Where(x => Json.ToNullableBoolean(x.Message, "$.Started") == true)
					.Select(x => Json.ToNullableBoolean(x.Message, "$.Started"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new bool?[] { true, true };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableDateTime()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
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

				var expected = new[] { "{ \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\", \"Started\": true }" };
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
				using var database = GenerateData(provider);
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
					? new short?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new short?[] { null, null, null, null, 29, null, 45, null, 32 };

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
				using var database = GenerateData(provider);
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
					? new int?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new int?[] { null, null, null, null, 29, null, 45, null, 32 };

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
				using var database = GenerateData(provider);
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
					? new long?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new long?[] { null, null, null, null, 29, null, 45, null, 32 };

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
				using var database = GenerateData(provider);
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
					? new ushort?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new ushort?[] { null, null, null, null, 29, null, 45, null, 32 };

				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ToNullableUInt32()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
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
					? new uint?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new uint?[] { null, null, null, null, 29, null, 45, null, 32 };

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
				using var database = GenerateData(provider);
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
					? new ulong?[] { null, 0, null, null, 29, null, 45, null, 32 }
					: new ulong?[] { null, null, null, null, 29, null, 45, null, 32 };

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
				using var database = GenerateData(provider);
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
				using var database = GenerateData(provider);
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
				using var database = GenerateData(provider);

				if (database.GetDatabaseType() == DatabaseType.Sqlite)
				{
					Console.WriteLine("\tNot Supported");
					return;
				}

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
				using var database = GenerateData(provider);
				var query = database.LogEvents
					.Select(x => Json.Value(x.Message, "$.FirstName"))
					.AsQueryable();

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { null, null, null, null, "Jane", null, "Bob", null, "Hello" };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);

				query = database.LogEvents
					.OrderBy(x => Json.Value(x.Message, "$.FirstName"))
					.Select(x => Json.Value(x.Message, "$.FirstName"));

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				expected = new[] { null, null, null, null, null, null, "Bob", "Hello", "Jane" };
				actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	[TestMethod]
	public void ValueOfDifferentStructureShouldFilter()
	{
		TestHelper.GetDataContexts(initialize: false)
			.ForEach(provider =>
			{
				using var database = GenerateData(provider);
				var query = database.LogEvents
					.Select(x => Json.Value(x.Message, "$.FirstName"))
					.Where(x => (x != null) && (x.Length <= 3));

				if (database is EntityFrameworkDatabase)
				{
					query.ToSql().Dump();
				}

				var expected = new[] { "Bob" };
				var actual = query.ToArray();
				actual.DumpJson();

				AreEqual(expected, actual);
			});
	}

	private IContosoDatabase GenerateData(IDatabaseProvider<IContosoDatabase> provider)
	{
		var database = provider.GetDatabase();
		Console.WriteLine(database.GetType().Name);

		database.LogEvents.Add(new LogEventEntity { Message = "{ \"StartedOn\": \"2023-04-21T15:34:33.2187670Z\", \"Started\": true }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"Name\": \"John Doe\", \"Age\": [24, 263] }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"StartedOn\": \"2022-04-21T15:34:33.2187670Z\", \"Started\": 1 }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"StartedOn\": 1.23, \"Started\": \"Not A Number\" }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Jane\", \"LastName\": \"Doe\", \"Age\": 29 }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"StartedOn\": true, \"Started\": null }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Bob\", \"LastName\": \"Bar\", \"Age\": 45 }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"StartedOn\": false, \"Started\": [23,45] }" });
		database.LogEvents.Add(new LogEventEntity { Message = "{ \"FirstName\": \"Hello\", \"LastName\": \"World\", \"Age\": 32 }" });
		database.SaveChanges();
		return database;
	}

	#endregion
}