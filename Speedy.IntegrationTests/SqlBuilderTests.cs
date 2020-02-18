#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SqlBuilderTests
	{
		#region Methods

		[TestMethod]
		public void GenerateSqliteQuery()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();

			var filters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string s, SqliteParameter[] p)>
			{
				{ x => x.SyncId != Guid.Empty, ("DELETE FROM \"Addresses\" WHERE \"SyncId\" <> @param_0", new[] { new SqliteParameter("param_0", Guid.Empty) }) },
				{ x => !x.IsDeleted, ("DELETE FROM \"Addresses\" WHERE \"IsDeleted\" = 0", new SqliteParameter[0]) },
				{
					x => x.CreatedOn > DateTime.MinValue && x.ModifiedOn < DateTime.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"CreatedOn\" > @param_0 AND \"ModifiedOn\" < @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.DateTime2) { Value = DateTime.MinValue }, new SqliteParameter("param_1", SqlDbType.DateTime2) { Value = DateTime.MaxValue } })
				},
				{
					x => x.Id >= byte.MinValue && x.Id <= byte.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.BigInt) { Value = (long) byte.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = (long) byte.MaxValue } })
				},
				{
					x => x.Id >= ushort.MinValue && x.Id <= ushort.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.BigInt) { Value = (long) ushort.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue } })
				},
				{
					x => x.Id >= short.MinValue && x.Id <= short.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.BigInt) { Value = (long) short.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = (long) short.MaxValue } })
				},
				{
					x => x.Id >= uint.MinValue && x.Id <= uint.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.BigInt) { Value = (long) uint.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = (long) uint.MaxValue } })
				},
				{
					x => x.Id >= int.MinValue && x.Id <= int.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[] { new SqliteParameter("param_0", SqlDbType.BigInt) { Value = (long) int.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = (long) int.MaxValue } })
				},
				{
					x => x.Id >= long.MinValue && x.Id <= long.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"Id\" >= @param_0 AND \"Id\" <= @param_1",
						new[]
						{
							new SqliteParameter("param_0", SqlDbType.BigInt) { Value = long.MinValue }, new SqliteParameter("param_1", SqlDbType.BigInt) { Value = long.MaxValue }
						})
				}
			};

			filters.ForEach(filter =>
			{
				var (sql, parameters) = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(filter.Key));
				sql.Dump();
				Assert.AreEqual(filter.Value.s, sql);
				TestHelper.AreEqual(filter.Value.p, parameters.Cast<SqliteParameter>().ToArray());
			});
		}

		[TestMethod]
		public void GenerateSqlQuery()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();

			var filters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string s, SqlParameter[] p)>
			{
				{ x => x.SyncId != Guid.Empty, ("DELETE FROM [dbo].[Addresses] WHERE [SyncId] <> @param_0", new[] { new SqlParameter("param_0", Guid.Empty) }) },
				{ x => !x.IsDeleted, ("DELETE FROM [dbo].[Addresses] WHERE [IsDeleted] = 0", new SqlParameter[0]) },
				{
					x => x.CreatedOn > DateTime.MinValue && x.ModifiedOn < DateTime.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [CreatedOn] > @param_0 AND [ModifiedOn] < @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.DateTime2) { Value = DateTime.MinValue }, new SqlParameter("param_1", SqlDbType.DateTime2) { Value = DateTime.MaxValue } })
				},
				{
					x => x.Id >= byte.MinValue && x.Id <= byte.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.BigInt) { Value = (long) byte.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = (long) byte.MaxValue } })
				},
				{
					x => x.Id >= ushort.MinValue && x.Id <= ushort.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.BigInt) { Value = (long) ushort.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue } })
				},
				{
					x => x.Id >= short.MinValue && x.Id <= short.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.BigInt) { Value = (long) short.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = (long) short.MaxValue } })
				},
				{
					x => x.Id >= uint.MinValue && x.Id <= uint.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.BigInt) { Value = (long) uint.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = (long) uint.MaxValue } })
				},
				{
					x => x.Id >= int.MinValue && x.Id <= int.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[] { new SqlParameter("param_0", SqlDbType.BigInt) { Value = (long) int.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = (long) int.MaxValue } })
				},
				{
					x => x.Id >= long.MinValue && x.Id <= long.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [Id] >= @param_0 AND [Id] <= @param_1",
						new[]
						{
							new SqlParameter("param_0", SqlDbType.BigInt) { Value = long.MinValue }, new SqlParameter("param_1", SqlDbType.BigInt) { Value = long.MaxValue }
						})
				}
			};

			filters.ForEach(filter =>
			{
				var (sql, parameters) = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(filter.Key));
				sql.Dump();
				Assert.AreEqual(filter.Value.s, sql);
				TestHelper.AreEqual(filter.Value.p, parameters.Cast<SqlParameter>().ToArray());
			});
		}

		#endregion
	}
}