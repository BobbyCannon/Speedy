#region References

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
		public void SqlDeleteQuery()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();

			var accountFilters = new Dictionary<Expression<Func<AccountEntity, bool>>, (string query, string parameters, SqlParameter[] p)>
			{
				{ x => x.SyncId != Guid.Empty, ("DELETE FROM [dbo].[Accounts] WHERE [AccountSyncId] <> @p0", "", new[] { new SqlParameter("p0", Guid.Empty) }) },
				{ x => x.Id > 2, ("DELETE FROM [dbo].[Accounts] WHERE [AccountId] > @p0", "AccountId:p0", new[] { new SqlParameter("p0", 2) }) },
				{ x => x.Nickname == "Fred", ("DELETE FROM [dbo].[Accounts] WHERE [AccountNickname] = @p0", "AccountNickname:p0", new[] { new SqlParameter("p0", SqlDbType.Text) { DbType = DbType.AnsiString, Value = "Fred" } }) }
			};

			accountFilters.ForEach(filter =>
			{
				var actual = SqlBuilder.GetSqlDelete(database, database.Accounts.Where(filter.Key));
				actual.Query.ToString().Dump();
				Assert.AreEqual(filter.Value.query, actual.Query.ToString());
				TestHelper.AreEqual(filter.Value.p, actual.Parameters.Cast<SqlParameter>().ToArray());
				Assert.AreEqual(filter.Value.parameters, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			});
			
			// Address
			var addressFilters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string query, SqlParameter[] p)>
			{
				{
					x => x.SyncId != Guid.Empty, ("DELETE FROM [dbo].[Addresses] WHERE [AddressSyncId] <> @p0",
						new[] { new SqlParameter("p0", Guid.Empty) })
				},
				{
					x => !x.IsDeleted, ("DELETE FROM [dbo].[Addresses] WHERE [AddressIsDeleted] = 0",
						new SqlParameter[0])
				},
				{
					x => x.CreatedOn > DateTime.MinValue && x.ModifiedOn < DateTime.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressCreatedOn] > @p0 AND [AddressModifiedOn] < @p1",
						new[] { new SqlParameter("p0", SqlDbType.DateTime2) { Value = DateTime.MinValue }, new SqlParameter("p1", SqlDbType.DateTime2) { Value = DateTime.MaxValue } })
				},
				{
					x => x.Id >= byte.MinValue && x.Id <= byte.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) byte.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) byte.MaxValue } })
				},
				{
					x => x.Id >= ushort.MinValue && x.Id <= ushort.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) ushort.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue } })
				},
				{
					x => x.Id >= short.MinValue && x.Id <= short.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) short.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) short.MaxValue } })
				},
				{
					x => x.Id >= uint.MinValue && x.Id <= uint.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) uint.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) uint.MaxValue } })
				},
				{
					x => x.Id >= int.MinValue && x.Id <= int.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = (long) int.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = (long) int.MaxValue } })
				},
				{
					x => x.Id >= long.MinValue && x.Id <= long.MaxValue, ("DELETE FROM [dbo].[Addresses] WHERE [AddressId] >= @p0 AND [AddressId] <= @p1",
						new[] { new SqlParameter("p0", SqlDbType.BigInt) { Value = long.MinValue }, new SqlParameter("p1", SqlDbType.BigInt) { Value = long.MaxValue } })
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
			var expectedQuery = "INSERT INTO [dbo].[Addresses] ([AddressCity], [AddressCreatedOn], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressCreatedOn:p1, AddressIsDeleted:p2, AddressLineOne:p3, AddressLineTwo:p4, AddressLinkedAddressId:p5, AddressLinkedAddressSyncId:p6, AddressModifiedOn:p7, AddressPostal:p8, AddressState:p9, AddressSyncId:p10";
			var expectedParametersTypes = "AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
		}

		[TestMethod]
		public void SqlInsertOrUpdateWithEntity()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var expectedQuery = "SET NOCOUNT, XACT_ABORT ON;\r\nMERGE [dbo].[Addresses] WITH (HOLDLOCK) AS T\r\nUSING (SELECT @p10 as AddressSyncId) AS S\r\n\tON T.[AddressSyncId] = S.[AddressSyncId]\r\nWHEN MATCHED\r\n\tTHEN UPDATE SET [AddressCity] = @p0, [AddressIsDeleted] = @p1, [AddressLineOne] = @p2, [AddressLineTwo] = @p3, [AddressLinkedAddressId] = @p4, [AddressLinkedAddressSyncId] = @p5, [AddressModifiedOn] = @p6, [AddressPostal] = @p7, [AddressState] = @p8\r\nWHEN NOT MATCHED\r\n\tTHEN INSERT ([AddressCity], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressCreatedOn], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10);";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressIsDeleted:p1, AddressLineOne:p2, AddressLineTwo:p3, AddressLinkedAddressId:p4, AddressLinkedAddressSyncId:p5, AddressModifiedOn:p6, AddressPostal:p7, AddressState:p8, AddressCreatedOn:p9, AddressSyncId:p10";
			var expectedParametersTypes = "AddressCity:Text, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressCreatedOn:DateTime2, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
		}

		[TestMethod]
		public void SqlInsertWithEntity()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var address = new AddressEntity();
			var expected = "INSERT INTO [dbo].[Addresses] ([AddressCity], [AddressCreatedOn], [AddressIsDeleted], [AddressLineOne], [AddressLineTwo], [AddressLinkedAddressId], [AddressLinkedAddressSyncId], [AddressModifiedOn], [AddressPostal], [AddressState], [AddressSyncId]) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressCreatedOn:p1, AddressIsDeleted:p2, AddressLineOne:p3, AddressLineTwo:p4, AddressLinkedAddressId:p5, AddressLinkedAddressSyncId:p6, AddressModifiedOn:p7, AddressPostal:p8, AddressState:p9, AddressSyncId:p10";
			var expectedParametersTypes = "AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
		}

		[TestMethod]
		public void SqliteDeleteQuery()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();

			var filters = new Dictionary<Expression<Func<AddressEntity, bool>>, (string s, SqliteParameter[] p)>
			{
				{ x => x.SyncId != Guid.Empty, ("DELETE FROM \"Addresses\" WHERE \"AddressSyncId\" <> @p0", new[] { new SqliteParameter("p0", Guid.Empty) }) },
				{ x => !x.IsDeleted, ("DELETE FROM \"Addresses\" WHERE \"AddressIsDeleted\" = 0", new SqliteParameter[0]) },
				{
					x => x.CreatedOn > DateTime.MinValue && x.ModifiedOn < DateTime.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressCreatedOn\" > @p0 AND \"AddressModifiedOn\" < @p1",
						new[] { new SqliteParameter("p0", SqlDbType.DateTime2) { Value = DateTime.MinValue }, new SqliteParameter("p1", SqlDbType.DateTime2) { Value = DateTime.MaxValue } })
				},
				{
					x => x.Id >= byte.MinValue && x.Id <= byte.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) byte.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) byte.MaxValue } })
				},
				{
					x => x.Id >= ushort.MinValue && x.Id <= ushort.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) ushort.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) ushort.MaxValue } })
				},
				{
					x => x.Id >= short.MinValue && x.Id <= short.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) short.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) short.MaxValue } })
				},
				{
					x => x.Id >= uint.MinValue && x.Id <= uint.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) uint.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) uint.MaxValue } })
				},
				{
					x => x.Id >= int.MinValue && x.Id <= int.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[] { new SqliteParameter("p0", SqlDbType.BigInt) { Value = (long) int.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = (long) int.MaxValue } })
				},
				{
					x => x.Id >= long.MinValue && x.Id <= long.MaxValue, ("DELETE FROM \"Addresses\" WHERE \"AddressId\" >= @p0 AND \"AddressId\" <= @p1",
						new[]
						{
							new SqliteParameter("p0", SqlDbType.BigInt) { Value = long.MinValue }, new SqliteParameter("p1", SqlDbType.BigInt) { Value = long.MaxValue }
						})
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
			var expected = "UPDATE \"Addresses\" SET \"AddressCity\" = @p0, \"AddressIsDeleted\" = @p1, \"AddressLineOne\" = @p2, \"AddressLineTwo\" = @p3, \"AddressLinkedAddressId\" = @p4, \"AddressLinkedAddressSyncId\" = @p5, \"AddressModifiedOn\" = @p6, \"AddressPostal\" = @p7, \"AddressState\" = @p8 WHERE \"AddressSyncId\" = @p9;\r\nINSERT INTO \"Addresses\" (\"AddressCity\", \"AddressIsDeleted\", \"AddressLineOne\", \"AddressLineTwo\", \"AddressLinkedAddressId\", \"AddressLinkedAddressSyncId\", \"AddressModifiedOn\", \"AddressPostal\", \"AddressState\", \"AddressSyncId\", \"AddressCreatedOn\")\r\nSELECT @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10\r\nWHERE (SELECT Changes() = 0);";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressIsDeleted:p1, AddressLineOne:p2, AddressLineTwo:p3, AddressLinkedAddressId:p4, AddressLinkedAddressSyncId:p5, AddressModifiedOn:p6, AddressPostal:p7, AddressState:p8, AddressSyncId:p9, AddressCreatedOn:p10";
			var expectedParametersTypes = "AddressCity:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text, AddressCreatedOn:Text";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsertOrUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));
		}

		[TestMethod]
		public void SqliteInsertWithEntity()
		{
			var provider = TestHelper.GetSqliteProvider();
			using var database = (ContosoSqliteDatabase) provider.GetDatabase();
			var address = new AddressEntity();
			var expected = "INSERT INTO \"Addresses\" (\"AddressCity\", \"AddressCreatedOn\", \"AddressIsDeleted\", \"AddressLineOne\", \"AddressLineTwo\", \"AddressLinkedAddressId\", \"AddressLinkedAddressSyncId\", \"AddressModifiedOn\", \"AddressPostal\", \"AddressState\", \"AddressSyncId\") VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressCreatedOn:p1, AddressIsDeleted:p2, AddressLineOne:p3, AddressLineTwo:p4, AddressLinkedAddressId:p5, AddressLinkedAddressSyncId:p6, AddressModifiedOn:p7, AddressPostal:p8, AddressState:p9, AddressSyncId:p10";
			var expectedParametersTypes = "AddressCity:Text, AddressCreatedOn:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlInsert(database, address);
			timer.Elapsed.Dump();
			Assert.AreEqual(expected, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));
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
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));

			// Check Address
			expectedQuery = "UPDATE \"Addresses\" SET \"AddressCity\" = @p0, \"AddressCreatedOn\" = @p1, \"AddressIsDeleted\" = @p2, \"AddressLineOne\" = @p3, \"AddressLineTwo\" = @p4, \"AddressLinkedAddressId\" = @p5, \"AddressLinkedAddressSyncId\" = @p6, \"AddressModifiedOn\" = @p7, \"AddressPostal\" = @p8, \"AddressState\" = @p9, \"AddressSyncId\" = @p10 WHERE \"AddressId\" = @p11";
			expectedParametersToColumnNames = "AddressCity:p0, AddressCreatedOn:p1, AddressIsDeleted:p2, AddressLineOne:p3, AddressLineTwo:p4, AddressLinkedAddressId:p5, AddressLinkedAddressSyncId:p6, AddressModifiedOn:p7, AddressPostal:p8, AddressState:p9, AddressSyncId:p10, AddressId:p11";
			expectedParametersTypes = "AddressCity:Text, AddressCreatedOn:Text, AddressIsDeleted:Integer, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:Integer, AddressLinkedAddressSyncId:Text, AddressModifiedOn:Text, AddressPostal:Text, AddressState:Text, AddressSyncId:Text, AddressId:Integer";
			timer.Restart();
			actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqliteParameter) x.Value).SqliteType)));
		}

		[TestMethod]
		public void SqlUpdate()
		{
			var provider = TestHelper.GetSqlProvider();
			using var database = (ContosoSqlDatabase) provider.GetDatabase();
			var expectedQuery = "UPDATE [dbo].[Addresses] SET [AddressCity] = @p0, [AddressCreatedOn] = @p1, [AddressIsDeleted] = @p2, [AddressLineOne] = @p3, [AddressLineTwo] = @p4, [AddressLinkedAddressId] = @p5, [AddressLinkedAddressSyncId] = @p6, [AddressModifiedOn] = @p7, [AddressPostal] = @p8, [AddressState] = @p9, [AddressSyncId] = @p10 WHERE [AddressId] = @p11";
			var expectedParametersToColumnNames = "AddressCity:p0, AddressCreatedOn:p1, AddressIsDeleted:p2, AddressLineOne:p3, AddressLineTwo:p4, AddressLinkedAddressId:p5, AddressLinkedAddressSyncId:p6, AddressModifiedOn:p7, AddressPostal:p8, AddressState:p9, AddressSyncId:p10, AddressId:p11";
			var expectedParametersTypes = "AddressCity:Text, AddressCreatedOn:DateTime2, AddressIsDeleted:Bit, AddressLineOne:Text, AddressLineTwo:Text, AddressLinkedAddressId:BigInt, AddressLinkedAddressSyncId:UniqueIdentifier, AddressModifiedOn:DateTime2, AddressPostal:Text, AddressState:Text, AddressSyncId:UniqueIdentifier, AddressId:BigInt";
			var timer = Stopwatch.StartNew();
			var actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
			timer.Restart();
			actual = SqlBuilder.GetSqlUpdate<AddressEntity>(database);
			timer.Elapsed.Dump();
			Assert.AreEqual(expectedQuery, actual.Query.ToString());
			Assert.AreEqual(expectedParametersToColumnNames, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).ParameterName)));
			Assert.AreEqual(expectedParametersTypes, string.Join(", ", actual.ParametersByColumnName.Select(x => x.Key + ":" + ((SqlParameter) x.Value).SqlDbType)));
		}

		#endregion
	}
}