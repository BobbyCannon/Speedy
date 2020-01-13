#region References

using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.EntityFramework.Sql;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SqlExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void ToSql()
		{
			var provider = TestHelper.GetSqlProvider();

			using (var database = provider.GetDatabase())
			{
				var expected = "SELECT [a].[Id], [a].[City], [a].[CreatedOn], [a].[IsDeleted], [a].[Line1], [a].[Line2], [a].[LinkedAddressId], [a].[LinkedAddressSyncId], [a].[ModifiedOn], [a].[Postal], [a].[State], [a].[SyncId]\r\nFROM [dbo].[Addresses] AS [a]";
				var actual = database.Addresses.ToSql();
				Assert.AreEqual(expected, actual);
			}

			provider = TestHelper.GetSqliteProvider();
			
			using (var database = provider.GetDatabase())
			{
				var expected = "SELECT \"a\".\"Id\", \"a\".\"City\", \"a\".\"CreatedOn\", \"a\".\"IsDeleted\", \"a\".\"Line1\", \"a\".\"Line2\", \"a\".\"LinkedAddressId\", \"a\".\"LinkedAddressSyncId\", \"a\".\"ModifiedOn\", \"a\".\"Postal\", \"a\".\"State\", \"a\".\"SyncId\"\r\nFROM \"Addresses\" AS \"a\"";
				var actual = database.Addresses.ToSql();
				Assert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void SqlDelete()
		{
			var provider = TestHelper.GetSqlProvider();

			using (var database = (ContosoSqlDatabase) provider.GetDatabase())
			{
				TestHelper.ExpectedException<ArgumentException>(() => SqlBuilder.GetSqlDelete(database, database.Addresses), "Must have a filter query");
				
				var expected = "DELETE FROM [dbo].[Addresses] WHERE [IsDeleted] = 0";
				var actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => !x.IsDeleted));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] = @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id == 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] > @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id > 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id >= 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] < @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id < 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] <= @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id <= 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] <> @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id != 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] > @param_0 AND [Id] < @param_1";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id > 5 && x.Id < 10));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [Id] < @param_0 OR [Id] > @param_1";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id < 5 || x.Id > 10));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [City] IS NULL";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.City == null));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM [dbo].[Addresses] WHERE [City] IS NOT NULL";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.City != null));
				Assert.AreEqual(expected, actual.Item1);
			}
		}

		[TestMethod]
		public void SqliteDelete()
		{
			var provider = TestHelper.GetSqliteProvider();

			using (var database = (ContosoSqliteDatabase) provider.GetDatabase())
			{
				TestHelper.ExpectedException<ArgumentException>(() => SqlBuilder.GetSqlDelete(database, database.Addresses), "Must have a filter query");
				
				var expected = "DELETE FROM \"Addresses\" WHERE \"IsDeleted\" = 0";
				var actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => !x.IsDeleted));
				Assert.AreEqual(expected, actual.Item1);

				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" = @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id == 5));
				Assert.AreEqual(expected, actual.Item1);

				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" > @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id > 5));
				Assert.AreEqual(expected, actual.Item1);

				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id >= 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" < @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id < 5));
				Assert.AreEqual(expected, actual.Item1);

				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" <= @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id <= 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" <> @param_0";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id != 5));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" > @param_0 AND \"Id\" < @param_1";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id > 5 && x.Id < 10));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM \"Addresses\" WHERE \"Id\" < @param_0 OR \"Id\" > @param_1";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.Id < 5 || x.Id > 10));
				Assert.AreEqual(expected, actual.Item1);

				expected = "DELETE FROM \"Addresses\" WHERE \"City\" IS NULL";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.City == null));
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "DELETE FROM \"Addresses\" WHERE \"City\" IS NOT NULL";
				actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(x => x.City != null));
				Assert.AreEqual(expected, actual.Item1);
			}
		}

		[TestMethod]
		public void SqliteUpdate()
		{
			var provider = TestHelper.GetSqliteProvider();
			
			using (var database = (ContosoSqliteDatabase) provider.GetDatabase())
			{
				var expected = "UPDATE \"Addresses\" SET \"IsDeleted\" = @param_0";
				var actual = SqlBuilder.GetSqlUpdate(database, database.Addresses, x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);

				expected = "UPDATE \"Addresses\" SET \"IsDeleted\" = @param_0 WHERE \"IsDeleted\" = 0";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => !x.IsDeleted), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);

				expected = "UPDATE \"Addresses\" SET \"IsDeleted\" = @param_0 WHERE \"IsDeleted\" = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.IsDeleted == false), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqliteParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual(true, ((SqliteParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqliteParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual(false, ((SqliteParameter) actual.Item2[1]).Value);
				
				expected = "UPDATE \"Addresses\" SET \"IsDeleted\" = @param_0 WHERE \"City\" = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.City == "Foo"), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqliteParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual(true, ((SqliteParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqliteParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual("Foo", ((SqliteParameter) actual.Item2[1]).Value);
				
				expected = "UPDATE \"Addresses\" SET \"City\" = @param_0 WHERE \"City\" = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.City == "foo"), x => new AddressEntity { City = "bar" });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqliteParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual("bar", ((SqliteParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqliteParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual("foo", ((SqliteParameter) actual.Item2[1]).Value);
			}
		}

		[TestMethod]
		public void SqlUpdate()
		{
			var provider = TestHelper.GetSqlProvider();
			
			using (var database = (ContosoSqlDatabase) provider.GetDatabase())
			{
				var expected = "UPDATE [a] SET [a].[IsDeleted] = @param_0 FROM [dbo].[Addresses] AS [a]";
				var actual = SqlBuilder.GetSqlUpdate(database, database.Addresses, x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);
				
				expected = "UPDATE [x] SET [x].[IsDeleted] = @param_0 FROM [dbo].[Addresses] AS [x] WHERE [x].[IsDeleted] = 0";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => !x.IsDeleted), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);

				expected = "UPDATE [x] SET [x].[IsDeleted] = @param_0 FROM [dbo].[Addresses] AS [x] WHERE [x].[IsDeleted] = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.IsDeleted == false), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqlParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual(true, ((SqlParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqlParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual(false, ((SqlParameter) actual.Item2[1]).Value);
				
				expected = "UPDATE [x] SET [x].[IsDeleted] = @param_0 FROM [dbo].[Addresses] AS [x] WHERE [x].[City] = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.City == "Foo"), x => new AddressEntity { IsDeleted = true });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqlParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual(true, ((SqlParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqlParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual("Foo", ((SqlParameter) actual.Item2[1]).Value);
				
				expected = "UPDATE [x] SET [x].[City] = @param_0 FROM [dbo].[Addresses] AS [x] WHERE [x].[City] = @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.City == "foo"), x => new AddressEntity { City = "bar" });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqlParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual("bar", ((SqlParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqlParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual("foo", ((SqlParameter) actual.Item2[1]).Value);
				
				expected = "UPDATE [x] SET [x].[City] = @param_0 FROM [dbo].[Addresses] AS [x] WHERE [x].[Id] > @param_1";
				actual = SqlBuilder.GetSqlUpdate(database, database.Addresses.Where(x => x.Id > 5), x => new AddressEntity { City = "bar" });
				Assert.AreEqual(expected, actual.Item1);
				Assert.AreEqual(2, actual.Item2.Count);
				Assert.AreEqual("param_0", ((SqlParameter) actual.Item2[0]).ParameterName);
				Assert.AreEqual("bar", ((SqlParameter) actual.Item2[0]).Value);
				Assert.AreEqual("param_1", ((SqlParameter) actual.Item2[1]).ParameterName);
				Assert.AreEqual((long) 5, ((SqlParameter) actual.Item2[1]).Value);
			}
		}

		#endregion
	}
}