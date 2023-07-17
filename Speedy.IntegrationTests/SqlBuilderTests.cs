#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.SyncApi;
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.UnitTests;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SqlBuilderTests : SpeedyTest
	{
		#region Methods

		[TestMethod]
		public void EscapeString()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();

			var scenarios = new[]
			{
				"John's", "'", "\"", "email: \"test@domain.com\"", "\'"
			};

			foreach (var scenario in scenarios)
			{
				scenario.Dump();

				database.Database.ExecuteSqlRaw("DELETE FROM [dbo].[Accounts]");
				database.Database.ExecuteSqlRaw("DELETE FROM [dbo].[Addresses]");

				var query = "INSERT INTO [dbo].[Addresses] ([AccountId], [AddressAccountSyncId], [AddressCity], [AddressCreatedOn], " +
					"[AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], " +
					"[AddressModifiedOn], [AddressPostal], [AddressState], [AddressSyncId]) " +
					"VALUES (/* AccountId */ null, /* AddressAccountSyncId */ null, /* AddressCity */ 'City', " +
					"/* AddressCreatedOn */ CAST(N'2023-07-17 13:42:03.444033' AS datetime2), /* AddressIsDeleted */ 1, " +
					$"'{SqlBuilder.EscapeString(scenario)}', /* AddressLineTwo */ 'AddressLineTwo', " +
					"/* AddressLinkedAddressId */ null, /* AddressLinkedAddressSyncId */ null, " +
					"/* AddressModifiedOn */ CAST(N'2023-07-17 13:42:03.444051' AS datetime2), " +
					"/* AddressPostal */ '12345', /* AddressState */ 'AB', " +
					"/* AddressSyncId */ NEWID());";

				database.Database.ExecuteSqlRaw(query);

				var address = database.Addresses.FirstOrDefault();
				Assert.IsNotNull(address);
				AreEqual(scenario, address.Line1);
			}
		}

		[TestMethod]
		public void SqlDeleteQuery()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var deletedOn = new DateTime(2022, 02, 05);

			var accountFilters = new Dictionary<Expression<Func<AccountEntity, bool>>, (string query, string parameters, SqlParameter[] p)>
			{
				{
					x => x.IsDeleted && ((x.Id == 2) || (x.Id == 67)), (
						"DELETE FROM [dbo].[Accounts] WHERE (([AccountIsDeleted] = 1) AND (([AccountId] = @p0) OR ([AccountId] = @p1)))",
						"AccountId:p0,p1", new[] { new SqlParameter("p0", 2), new SqlParameter("p1", 67) }
					)
				},
				{
					x => x.SyncId != Guid.Empty,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountSyncId] <> @p0)",
						"AccountSyncId:p0", new[] { new SqlParameter("p0", Guid.Empty) })
				},
				{
					x => x.Id > 2,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountId] > @p0)",
						"AccountId:p0", new[] { new SqlParameter("p0", 2) })
				},
				{
					x => x.Nickname == "Fred",
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountNickname] = @p0)",
						"AccountNickname:p0", new[] { new SqlParameter("p0", SqlDbType.Text) { DbType = DbType.AnsiString, Value = "Fred" } })
				},
				{
					x => x.IsDeleted,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountIsDeleted] = 1)",
						"", Array.Empty<SqlParameter>())
				},
				// ReSharper disable once RedundantBoolCompare
				{
					x => x.IsDeleted == true,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountIsDeleted] = @p0)",
						"AccountIsDeleted:p0", new[] { new SqlParameter("p0", true) })
				},
				{
					x => !x.IsDeleted,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountIsDeleted] = 0)",
						"", Array.Empty<SqlParameter>())
				},
				{
					x => x.IsDeleted == false,
					("DELETE FROM [dbo].[Accounts] WHERE ([AccountIsDeleted] = @p0)",
						"AccountIsDeleted:p0", new[] { new SqlParameter("p0", false) })
				},
				{
					x => x.IsDeleted && (x.ModifiedOn <= deletedOn),
					("DELETE FROM [dbo].[Accounts] WHERE (([AccountIsDeleted] = 1) AND ([AccountModifiedOn] <= @p0))",
						"AccountModifiedOn:p0", new[] { new SqlParameter("p0", SqlDbType.DateTime2) { DbType = DbType.DateTime2, Value = deletedOn } })
				},
				// ReSharper disable once RedundantBoolCompare
				{
					x => (x.IsDeleted == true) && (x.ModifiedOn <= deletedOn),
					("DELETE FROM [dbo].[Accounts] WHERE (([AccountIsDeleted] = @p0) AND ([AccountModifiedOn] <= @p1))",
						"AccountIsDeleted:p0, AccountModifiedOn:p1",
						new[] { new SqlParameter("p0", SqlDbType.Bit) { Value = true }, new SqlParameter("p1", SqlDbType.DateTime2) { DbType = DbType.DateTime2, Value = deletedOn } })
				},
				{
					x => !x.IsDeleted && (x.ModifiedOn <= deletedOn),
					("DELETE FROM [dbo].[Accounts] WHERE (([AccountIsDeleted] = 0) AND ([AccountModifiedOn] <= @p0))",
						"AccountModifiedOn:p0",
						new[] { new SqlParameter("p0", SqlDbType.DateTime2) { DbType = DbType.DateTime2, Value = deletedOn } })
				},
				{
					x => (x.IsDeleted == false) && (x.ModifiedOn <= deletedOn),
					("DELETE FROM [dbo].[Accounts] WHERE (([AccountIsDeleted] = @p0) AND ([AccountModifiedOn] <= @p1))",
						"AccountIsDeleted:p0, AccountModifiedOn:p1",
						new[] { new SqlParameter("p0", SqlDbType.Bit) { Value = false }, new SqlParameter("p1", SqlDbType.DateTime2) { DbType = DbType.DateTime2, Value = deletedOn } })
				}
			};

			accountFilters.ForEach(filter =>
			{
				var actual = SqlBuilder.GetSqlDelete(database, database.Accounts.Where(filter.Key));
				actual.Query.ToString().Dump();
				Assert.AreEqual(filter.Value.query, actual.Query.ToString());
				TestHelper.AreEqual(filter.Value.p, actual.Parameters.Cast<SqlParameter>().ToArray());
				Assert.AreEqual(filter.Value.parameters, GetParameterNames(actual.ParametersByColumnName));
			});

			// Address
			var addressFilters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string query, SqlParameter[] p)>
			{
				{
					x => x.SyncId != Guid.Empty,
					("DELETE FROM [dbo].[Addresses] WHERE ([AddressSyncId] <> @p0)",
						new[] { new SqlParameter("p0", Guid.Empty) })
				},
				{
					x => !x.IsDeleted,
					("DELETE FROM [dbo].[Addresses] WHERE ([AddressIsDeleted] = 0)",
						Array.Empty<SqlParameter>())
				},
				{
					x => (x.CreatedOn > DateTime.MinValue) && (x.ModifiedOn < DateTime.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressCreatedOn] > @p0) AND ([AddressModifiedOn] < @p1))",
						new[]
						{
							new SqlParameter("p0", SqlDbType.DateTime2) { Value = DateTime.MinValue },
							new SqlParameter("p1", SqlDbType.DateTime2) { Value = DateTime.MaxValue }
						})
				},
				{
					x => (x.Id >= byte.MinValue) && (x.Id <= byte.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))",
						new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) byte.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) byte.MaxValue }
						})
				},
				{
					x => (x.Id >= ushort.MinValue) && (x.Id <= ushort.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))"
						, new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) ushort.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue }
						})
				},
				{
					x => (x.Id >= short.MinValue) && (x.Id <= short.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))"
						, new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) short.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) short.MaxValue }
						})
				},
				{
					x => (x.Id >= uint.MinValue) && (x.Id <= uint.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))"
						, new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) uint.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) uint.MaxValue }
						})
				},
				{
					x => (x.Id >= int.MinValue) && (x.Id <= int.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))"
						, new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) int.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) int.MaxValue }
						})
				},
				{
					x => (x.Id >= long.MinValue) && (x.Id <= long.MaxValue),
					("DELETE FROM [dbo].[Addresses] WHERE (([AddressId] >= @p0) AND ([AddressId] <= @p1))",
						new[]
						{
							new SqlParameter("p0", SqlDbType.BigInt) { Value = long.MinValue },
							new SqlParameter("p1", SqlDbType.BigInt) { Value = long.MaxValue }
						})
				}
			};

			addressFilters.ForEach(filter =>
			{
				var actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(filter.Key));
				actual.Query.ToString().Dump();
				Assert.AreEqual(filter.Value.query, actual.Query.ToString());
				TestHelper.AreEqual(filter.Value.p, actual.Parameters.Cast<SqlParameter>().ToArray());
			});
		}

		[TestMethod]
		public void SqlInsert()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var expectedQuery = "INSERT INTO [dbo].[Addresses] ([AccountId], [AddressAccountSyncId], [AddressCity], [AddressCreatedOn], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressCreatedOn:p3, AddressIsDeleted:p4, AddressLineOne:p5, AddressLineTwo:p6, AddressLinkedAddressId:p7, AddressLinkedAddressSyncId:p8, AddressModifiedOn:p9, AddressPostal:p10, AddressState:p11, AddressSyncId:p12";
			var expectedParametersTypes = "AccountId:Int, AddressAccountSyncId:UniqueIdentifier, AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert<AddressEntity>(database);
			timer.Elapsed.Dump();
			actual.Query.ToString().Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, GetParameterNames(actual.ParametersByColumnName));
			Assert.AreEqual(expectedParametersTypes, GetParameterTypes(actual.ParametersByColumnName));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, GetParameterNames(actual.ParametersByColumnName));
			Assert.AreEqual(expectedParametersTypes, GetParameterTypes(actual.ParametersByColumnName));
		}

		[TestMethod]
		public void SqlInsertOrUpdateWithEntity()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var expectedQuery = "SET NOCOUNT, XACT_ABORT ON;\r\nMERGE [dbo].[Addresses] WITH (HOLDLOCK) AS T\r\nUSING (SELECT @p12 as AddressSyncId) AS S\r\n\tON T.[AddressSyncId] = S.[AddressSyncId]\r\nWHEN MATCHED\r\n\tTHEN UPDATE SET [AccountId] = @p0, [AddressAccountSyncId] = @p1, [AddressCity] = @p2, [AddressIsDeleted] = @p3, [AddressLineOne] = @p4, [AddressLineTwo] = @p5, [AddressLinkedAddressId] = @p6, [AddressLinkedAddressSyncId] = @p7, [AddressModifiedOn] = @p8, [AddressPostal] = @p9, [AddressState] = @p10\r\nWHEN NOT MATCHED\r\n\tTHEN INSERT ([AccountId], [AddressAccountSyncId], [AddressCity], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressCreatedOn], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12);";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressIsDeleted:p3, AddressLineOne:p4, AddressLineTwo:p5, AddressLinkedAddressId:p6, AddressLinkedAddressSyncId:p7, AddressModifiedOn:p8, AddressPostal:p9, AddressState:p10, AddressCreatedOn:p11, AddressSyncId:p12";
			var expectedParametersTypes = "AccountId:Int, AddressAccountSyncId:UniqueIdentifier, AddressCity:Text, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressCreatedOn:DateTime2, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = GetParameterNames(actual.ParametersByColumnName);
			var actualParametersTypes = GetParameterTypes(actual.ParametersByColumnName);
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
			timer.Restart();
			actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, GetParameterNames(actual.ParametersByColumnName));
			Assert.AreEqual(expectedParametersTypes, GetParameterTypes(actual.ParametersByColumnName));
		}

		[TestMethod]
		public void SqlInsertWithEntity()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var address = new AddressEntity();
			var expectedQuery = "INSERT INTO [dbo].[Addresses] ([AccountId], [AddressAccountSyncId], [AddressCity], [AddressCreatedOn], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressCreatedOn:p3, AddressIsDeleted:p4, AddressLineOne:p5, AddressLineTwo:p6, AddressLinkedAddressId:p7, AddressLinkedAddressSyncId:p8, AddressModifiedOn:p9, AddressPostal:p10, AddressState:p11, AddressSyncId:p12";
			var expectedParametersTypes = "AccountId:Int, AddressAccountSyncId:UniqueIdentifier, AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = GetParameterNames(actual.ParametersByColumnName);
			var actualParametersTypes = GetParameterTypes(actual.ParametersByColumnName);
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, GetParameterNames(actual.ParametersByColumnName));
			Assert.AreEqual(expectedParametersTypes, GetParameterTypes(actual.ParametersByColumnName));
		}

		[TestMethod]
		public void SqliteDeleteQuery()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();

			var filters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string s, SqliteParameter[] p)>
			{
				{
					x => x.SyncId != Guid.Empty,
					("DELETE FROM \"Addresses\" WHERE (\"AddressSyncId\" <> @p0)",
						new[] { new SqliteParameter("p0", Guid.Empty) })
				},
				{
					x => !x.IsDeleted,
					("DELETE FROM \"Addresses\" WHERE (\"AddressIsDeleted\" = 0)",
						Array.Empty<SqliteParameter>())
				},
				{
					x => (x.CreatedOn > DateTime.MinValue) && (x.ModifiedOn < DateTime.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressCreatedOn\" > @p0) AND (\"AddressModifiedOn\" < @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.DateTime2) { Value = DateTime.MinValue }, new SqliteParameter("p1", SqlDbType.DateTime2) { Value = DateTime.MaxValue } })
				},
				{
					x => (x.Id >= byte.MinValue) && (x.Id <= byte.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) byte.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) byte.MaxValue } })
				},
				{
					x => (x.Id >= ushort.MinValue) && (x.Id <= ushort.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) ushort.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue } })
				},
				{
					x => (x.Id >= short.MinValue) && (x.Id <= short.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) short.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) short.MaxValue } })
				},
				{
					x => (x.Id >= uint.MinValue) && (x.Id <= uint.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) uint.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) uint.MaxValue } })
				},
				{
					x => (x.Id >= int.MinValue) && (x.Id <= int.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) int.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) int.MaxValue } })
				},
				{
					x => (x.Id >= long.MinValue) && (x.Id <= long.MaxValue),
					("DELETE FROM \"Addresses\" WHERE ((\"AddressId\" >= @p0) AND (\"AddressId\" <= @p1))",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = long.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = long.MaxValue } })
				}
			};

			filters.ForEach(filter =>
			{
				var actual = SqlBuilder.GetSqlDelete(database, database.Addresses.Where(filter.Key));
				actual.Query.ToString().Dump();
				Assert.AreEqual(filter.Value.s, actual.Query.ToString());
				TestHelper.AreEqual(filter.Value.p, actual.Parameters.Cast<SqliteParameter>().ToArray());
			});
		}

		[TestMethod]
		public void SqliteInsertOrUpdateWithEntity()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();
			var expectedQuery = "UPDATE \"Addresses\" SET \"AccountId\" = @p0, \"AddressAccountSyncId\" = @p1, \"AddressCity\" = @p2, \"AddressIsDeleted\" = @p3, \"AddressLineOne\" = @p4, \"AddressLineTwo\" = @p5, \"AddressLinkedAddressId\" = @p6, \"AddressLinkedAddressSyncId\" = @p7, \"AddressModifiedOn\" = @p8, \"AddressPostal\" = @p9, \"AddressState\" = @p10 WHERE \"AddressSyncId\" = @p11;\r\nINSERT INTO \"Addresses\" (\"AccountId\", \"AddressAccountSyncId\", \"AddressCity\", \"AddressIsDeleted\", \"AddressLineOne\", \"AddressLineTwo\", \"AddressLinkedAddressId\", \"AddressLinkedAddressSyncId\", \"AddressModifiedOn\", \"AddressPostal\", \"AddressState\", \"AddressSyncId\", \"AddressCreatedOn\")\r\nSELECT @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12\r\nWHERE (SELECT Changes() = 0);";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressIsDeleted:p3, AddressLineOne:p4, AddressLineTwo:p5, AddressLinkedAddressId:p6, AddressLinkedAddressSyncId:p7, AddressModifiedOn:p8, AddressPostal:p9, AddressState:p10, AddressSyncId:p11, AddressCreatedOn:p12";
			var expectedParametersTypes = "AccountId:Integer, AddressAccountSyncId:Text, AddressCity:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text, AddressCreatedOn:Text";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName));
			var actualParametersTypes = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType));
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
			timer.Restart();
			actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType)));
		}

		[TestMethod]
		public void SqliteInsertWithEntity()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();
			var address = new AddressEntity();
			var expectedQuery = "INSERT INTO \"Addresses\" (\"AccountId\", \"AddressAccountSyncId\", \"AddressCity\", \"AddressCreatedOn\", \"AddressIsDeleted\", \"AddressLineOne\", \"AddressLineTwo\", \"AddressLinkedAddressId\", \"AddressLinkedAddressSyncId\", \"AddressModifiedOn\", \"AddressPostal\", \"AddressState\", \"AddressSyncId\") VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressCreatedOn:p3, AddressIsDeleted:p4, AddressLineOne:p5, AddressLineTwo:p6, AddressLinkedAddressId:p7, AddressLinkedAddressSyncId:p8, AddressModifiedOn:p9, AddressPostal:p10, AddressState:p11, AddressSyncId:p12";
			var expectedParametersTypes = "AccountId:Integer, AddressAccountSyncId:Text, AddressCity:Text, AddressCreatedOn:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName));
			var actualParametersTypes = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType));
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType)));
		}

		[TestMethod]
		public void SqliteUpdate()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();

			// Check Account
			var expectedQuery = "UPDATE \"Accounts\" SET \"AccountAddressId\" = @p0, \"AccountAddressSyncId\" = @p1, \"AccountCreatedOn\" = @p2, \"AccountEmailAddress\" = @p3, \"AccountExternalId\" = @p4, \"AccountIsDeleted\" = @p5, \"AccountLastLoginDate\" = @p6, \"AccountModifiedOn\" = @p7, \"AccountName\" = @p8, \"AccountNickname\" = @p9, \"AccountPasswordHash\" = @p10, \"AccountRoles\" = @p11, \"AccountSyncId\" = @p12 WHERE \"AccountId\" = @p13";
			var expectedParametersToColumnNames = "AccountAddressId:p0, AccountAddressSyncId:p1, AccountCreatedOn:p2, AccountEmailAddress:p3, AccountExternalId:p4, AccountIsDeleted:p5, AccountLastLoginDate:p6, AccountModifiedOn:p7, AccountName:p8, AccountNickname:p9, AccountPasswordHash:p10, AccountRoles:p11, AccountSyncId:p12, AccountId:p13";
			var expectedParametersTypes = "AccountAddressId:Integer, AccountAddressSyncId:Text, AccountCreatedOn:Text, AccountEmailAddress:Text, AccountExternalId:Text, AccountIsDeleted:Integer, AccountLastLoginDate:Text, AccountModifiedOn:Text, AccountName:Text, AccountNickname:Text, AccountPasswordHash:Text, AccountRoles:Text, AccountSyncId:Text, AccountId:Integer";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlUpdate<AccountEntity>(database);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName));
			var actualParametersTypes = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType));
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);

			// Check Address
			expectedQuery = "UPDATE \"Addresses\" SET \"AccountId\" = @p0, \"AddressAccountSyncId\" = @p1, \"AddressCity\" = @p2, \"AddressCreatedOn\" = @p3, \"AddressIsDeleted\" = @p4, \"AddressLineOne\" = @p5, \"AddressLineTwo\" = @p6, \"AddressLinkedAddressId\" = @p7, \"AddressLinkedAddressSyncId\" = @p8, \"AddressModifiedOn\" = @p9, \"AddressPostal\" = @p10, \"AddressState\" = @p11, \"AddressSyncId\" = @p12 WHERE \"AddressId\" = @p13";
			expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressCreatedOn:p3, AddressIsDeleted:p4, AddressLineOne:p5, AddressLineTwo:p6, AddressLinkedAddressId:p7, AddressLinkedAddressSyncId:p8, AddressModifiedOn:p9, AddressPostal:p10, AddressState:p11, AddressSyncId:p12, AddressId:p13";
			expectedParametersTypes = "AccountId:Integer, AddressAccountSyncId:Text, AddressCity:Text, AddressCreatedOn:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text, AddressId:Integer";
			timer.Restart();
			actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			actualQuery = actual.Query.ToString();
			actualParametersToColumnNames = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).ParameterName));
			actualParametersTypes = string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value.First()).SqliteType));
			$"expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
		}

		[TestMethod]
		public void SqlUpdate()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var test = new { Message = "foo bar" };
			var actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => x.Level == LogLevel.Critical), entity => new LogEventEntity { Message = test.Message });
			var expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE ([Level] = @p1)";
			var actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery, actualQuery);
			Assert.AreEqual(2, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());
			Assert.AreEqual(SqlDbType.Int, ((SqlParameter) actual.Parameters[1]).SqlDbType);
			Assert.AreEqual("0", ((SqlParameter) actual.Parameters[1]).SqlValue.ToString());
		}

		[TestMethod]
		public void SqlUpdateWithBooleans()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var test = new { Message = "foo bar" };
			var actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => x.IsDeleted), entity => new LogEventEntity { Message = test.Message });
			var expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE ([IsDeleted] = 1)";
			var actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(1, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());

			actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => !x.IsDeleted), entity => new LogEventEntity { Message = test.Message });
			expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE ([IsDeleted] = 0)";
			actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(1, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());

			// ReSharper disable once RedundantBoolCompare
			actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => x.IsDeleted == true), entity => new LogEventEntity { Message = test.Message });
			expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE ([IsDeleted] = @p1)";
			actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(2, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());
			Assert.AreEqual(SqlDbType.Bit, ((SqlParameter) actual.Parameters[1]).SqlDbType);
			Assert.AreEqual("True", ((SqlParameter) actual.Parameters[1]).SqlValue.ToString());

			actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => x.IsDeleted == false), entity => new LogEventEntity { Message = test.Message });
			expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE ([IsDeleted] = @p1)";
			actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(2, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());
			Assert.AreEqual(SqlDbType.Bit, ((SqlParameter) actual.Parameters[1]).SqlDbType);
			Assert.AreEqual("False", ((SqlParameter) actual.Parameters[1]).SqlValue.ToString());

			actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => (x.Level == LogLevel.Critical) && x.IsDeleted), entity => new LogEventEntity { Message = test.Message });
			expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE (([Level] = @p1) AND ([IsDeleted] = 1))";
			actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(2, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());
			Assert.AreEqual(SqlDbType.Int, ((SqlParameter) actual.Parameters[1]).SqlDbType);
			Assert.AreEqual("0", ((SqlParameter) actual.Parameters[1]).SqlValue.ToString());

			actual = SqlBuilder.GetSqlUpdate(database, database.LogEvents.Where(x => (x.Level == LogLevel.Critical) && !x.IsDeleted), entity => new LogEventEntity { Message = test.Message });
			expected = "UPDATE [dbo].[LogEvents] SET [Message] = @p0 WHERE (([Level] = @p1) AND ([IsDeleted] = 0))";
			actualQuery = actual.Query.ToString();
			Assert.AreEqual(expected, actualQuery);
			Assert.AreEqual(2, actual.Parameters.Count);
			Assert.AreEqual(SqlDbType.Text, ((SqlParameter) actual.Parameters[0]).SqlDbType);
			Assert.AreEqual("foo bar", ((SqlParameter) actual.Parameters[0]).SqlValue.ToString());
			Assert.AreEqual(SqlDbType.Int, ((SqlParameter) actual.Parameters[1]).SqlDbType);
			Assert.AreEqual("0", ((SqlParameter) actual.Parameters[1]).SqlValue.ToString());
		}

		[TestMethod]
		public void SqlUpdateWithoutQuery()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var expectedQuery = "UPDATE [dbo].[Addresses] SET [AccountId] = @p0, [AddressAccountSyncId] = @p1, [AddressCity] = @p2, [AddressCreatedOn] = @p3, [AddressIsDeleted] = @p4, [AddressLineOne] = @p5, [AddressLineTwo] = @p6, [AddressLinkedAddressId] = @p7, [AddressLinkedAddressSyncId] = @p8, [AddressModifiedOn] = @p9, [AddressPostal] = @p10, [AddressState] = @p11, [AddressSyncId] = @p12 WHERE [AddressId] = @p13";
			var expectedParametersToColumnNames = "AccountId:p0, AddressAccountSyncId:p1, AddressCity:p2, AddressCreatedOn:p3, AddressIsDeleted:p4, AddressLineOne:p5, AddressLineTwo:p6, AddressLinkedAddressId:p7, AddressLinkedAddressSyncId:p8, AddressModifiedOn:p9, AddressPostal:p10, AddressState:p11, AddressSyncId:p12, AddressId:p13";
			var expectedParametersTypes = "AccountId:Int, AddressAccountSyncId:UniqueIdentifier, AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier, AddressId:BigInt";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			var actualQuery = actual.Query.ToString();
			var actualParametersToColumnNames = GetParameterNames(actual.ParametersByColumnName);
			var actualParametersTypes = GetParameterTypes(actual.ParametersByColumnName);
			$"var expectedQuery = \"{actualQuery.Escape()}\";".Dump();
			$"var expectedParametersToColumnNames = \"{actualParametersToColumnNames.Escape()}\";".Dump();
			$"var expectedParametersTypes = \"{actualParametersTypes.Escape()}\";".Dump();
			Assert.AreEqual(expectedQuery, actualQuery);
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);

			timer.Restart();
			actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			actualParametersToColumnNames = GetParameterNames(actual.ParametersByColumnName);
			actualParametersTypes = GetParameterTypes(actual.ParametersByColumnName);
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, actualParametersToColumnNames);
			Assert.AreEqual(expectedParametersTypes, actualParametersTypes);
		}

		private string GetParameterNames(IDictionary<string, IList<object>> parametersByColumnName)
		{
			return string.Join(", ", parametersByColumnName.Select(x => x.Key + ":" + string.Join(",", x.Value.Select(v => ((SqlParameter) v).ParameterName))));
		}

		private string GetParameterTypes(IDictionary<string, IList<object>> parametersByColumnName)
		{
			return string.Join(", ", parametersByColumnName.Select(x => x.Key + ":" + string.Join(",", x.Value.Select(v => ((SqlParameter) v).SqlDbType))));
		}

		#endregion
	}
}